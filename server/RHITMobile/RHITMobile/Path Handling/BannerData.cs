﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RHITMobile.RhitPrivate;
using System.Collections.Specialized;
using System.Net;
using System.ServiceModel;
using RHITMobile.Properties;

namespace RHITMobile {
    public class BannerHandler : SecurePathHandler {
        public BannerHandler() {
            Redirects.Add("authenticate", new BannerAuthenticateHandler());
            Redirects.Add("user", new BannerUserHandler());
            Redirects.Add("course", new BannerCourseHandler());
            Redirects.Add("room", new BannerRoomHandler());
        }

        public static readonly Queue<BannerRequestRecord> RequestQueue = new Queue<BannerRequestRecord>();
        public static readonly Dictionary<string, List<BannerRequestRecord>> RequestDictionary = new Dictionary<string, List<BannerRequestRecord>>();
        public static readonly SortedSet<string> BlockedUsers = new SortedSet<string>();
        public static readonly Dictionary<string, BannerAuthenticationRecord> AuthenticatedUsers = new Dictionary<string, BannerAuthenticationRecord>();

        private static void CleanQueue() {
            while (RequestQueue.Any() && RequestQueue.Peek().Time <= DateTime.Now.AddHours(-24)) {
                var request = RequestQueue.Dequeue();
                RequestDictionary[request.Username].Remove(request);
                if (!RequestDictionary[request.Username].Any())
                    RequestDictionary.Remove(request.Username);
            }
        }

        public static void VerifyUser(ThreadInfo currentThread, string username) {
            CleanQueue();
            if (BlockedUsers.Contains(username)) {
                if (RequestDictionary[username].Sum(r => r.SecureServerRequests) >= Settings.Default.MaxDailyBannerCalls) {
                    throw new BadRequestException(currentThread, "Number of secure server calls has exceeded the daily limit for user '{0}'.", username);
                } else {
                    BlockedUsers.Remove(username);
                }
            }
        }

        public static void LogRequest(string token, string action, int secureServerRequests) {
            LogRequestRecord(AuthenticatedUsers.ContainsKey(token) ? AuthenticatedUsers[token].Username : token,
                action, secureServerRequests);
        }

        public static void LogAuthentication(string username, AuthenticationResponse auth) {
            LogRequestRecord(username, "Login attempt", 1);
            if (auth != null) {
                AuthenticatedUsers[auth.Token] = new BannerAuthenticationRecord(username, auth.Expiration);
            }
        }

        private static void LogRequestRecord(string username, string action, int secureServerRequests) {
            var record = new BannerRequestRecord(username, action, secureServerRequests, DateTime.Now);
            RequestQueue.Enqueue(record);
            if (!RequestDictionary.ContainsKey(username))
                RequestDictionary[username] = new List<BannerRequestRecord>();
            RequestDictionary[username].Add(record);

            if (RequestDictionary[username].Sum(r => r.SecureServerRequests) >= Settings.Default.MaxDailyBannerCalls)
                BlockedUsers.Add(username);
        }

        public static IEnumerable<ThreadInfo> ClearAuthenticationExpirations(ThreadManager TM) {
            var currentThread = TM.CurrentThread;
            int i = 0;
            while (true) {
                if (i < AuthenticatedUsers.Count) {
                    if (AuthenticatedUsers.ElementAt(i).Value.Expiration < DateTime.Now) {
                        AuthenticatedUsers.Remove(AuthenticatedUsers.ElementAt(i).Key);
                    } else {
                        yield return TM.Sleep(currentThread, 600000);
                        i++;
                    }
                } else {
                    i = 0;
                    yield return TM.Sleep(currentThread, 600000);
                }
            }
        }

        public class BannerRequestRecord {
            public string Username { get; private set; }
            public string Action { get; private set; }
            public int SecureServerRequests { get; private set; }
            public DateTime Time { get; private set; }

            public BannerRequestRecord(string username, string action, int secureServerRequests, DateTime time) {
                Username = username;
                Action = action;
                SecureServerRequests = secureServerRequests;
                Time = time;
            }
        }

        public class BannerAuthenticationRecord {
            public string Username { get; private set; }
            public DateTime Expiration { get; private set; }

            public BannerAuthenticationRecord(string username, DateTime exp) {
                Username = username;
                Expiration = exp;
            }
        }

        public static IEnumerable<ThreadInfo> VerifyToken(ThreadManager TM, NameValueCollection headers) {
            var currentThread = TM.CurrentThread;

            if (headers["Auth-Token"] == null) {
                throw new UnauthorizedException(currentThread, "An authentication token is required for this request.");
            } else {
                string token = headers["Auth-Token"];
                if (AuthenticatedUsers.ContainsKey(token))
                    VerifyUser(currentThread, AuthenticatedUsers[token].Username);
                else
                    VerifyUser(currentThread, token);
                yield return TM.Return(currentThread, token);
            }
        }

        public static IEnumerable<ThreadInfo> GetCourses(ThreadManager TM, Dictionary<string, string> query, string token, IEnumerable<KeyValuePair<int, int>> termsCrns) {
            var currentThread = TM.CurrentThread;
            var service = new WebServiceClient();
            int requests = 0;

            var courses = new List<Course>();
            foreach (var termCrn in termsCrns) {
                requests++;
                yield return TM.StartNewThread(currentThread, CatchAuthFailure(currentThread, () => service.GetCourse(token, termCrn.Key, termCrn.Value)));
                var course = TM.GetResult<Course>(currentThread);
                if (course != null) {
                    courses.Add(course);
                }
            }

            BannerHandler.LogRequest(token, "Getting course information", requests);
            yield return TM.Await(currentThread, GetCourses(TM, query, token, courses));
            yield return TM.Return(currentThread);
        }

        public static IEnumerable<ThreadInfo> GetCourses(ThreadManager TM, Dictionary<string, string> query, string token, IEnumerable<Course> courses) {
            var currentThread = TM.CurrentThread;
            var service = new WebServiceClient();
            int requests = 0;

            bool getEnrolled = false;
            if (query.ContainsKey("getenrolled"))
                Boolean.TryParse(query["getenrolled"], out getEnrolled);

            bool getSchedule = false;
            if (query.ContainsKey("getschedule"))
                Boolean.TryParse(query["getschedule"], out getSchedule);

            var response = new CoursesResponse();
            foreach (var course in courses) {
                var scourse = new SCourse(course);

                if (course.Instructor != null) {
                    requests++;
                    yield return TM.StartNewThread(currentThread, CatchAuthFailure(currentThread, () => service.GetUser(token, course.Instructor)));
                    var instructor = TM.GetResult<User>(currentThread);
                    if (instructor != null)
                        scourse.Instructor = new ShortUser(instructor);
                }

                if (getEnrolled) {
                    scourse.Students = new List<ShortUser>();
                    requests++;
                    yield return TM.StartNewThread(currentThread, CatchAuthFailure(currentThread, () => service.GetCourseEnrollment(token, course.Term, course.CRN)));
                    var students = TM.GetResult<string[]>(currentThread);
                    if (students != null) {
                        foreach (var username in students) {
                            requests++;
                            yield return TM.StartNewThread(currentThread, CatchAuthFailure(currentThread, () => service.GetUser(token, username)));
                            var student = TM.GetResult<User>(currentThread);
                            if (student != null)
                                scourse.Students.Add(new ShortUser(student));
                        }
                    }
                }

                if (getSchedule) {
                    requests++;
                    yield return TM.StartNewThread(currentThread, CatchAuthFailure(currentThread, () => service.GetCourseSchedule(token, course.Term, course.CRN)));
                    var times = TM.GetResult<CourseTime[]>(currentThread);
                    if (times != null) {
                        scourse.Schedule = times.Select(time => new CourseMeeting(time)).ToList();
                    } else {
                        scourse.Schedule = new List<CourseMeeting>();
                    }
                }

                response.Courses.Add(scourse);
            }

            BannerHandler.LogRequest(token, "Getting course instructors, schedules, and/or enrollments", requests);
            yield return TM.Return(currentThread, response);
        }

        public static Func<T> CatchAuthFailure<T>(ThreadInfo currentThread, Func<T> func) {
            return new Func<T>(() => {
                try {
                    try {
                        return func();
                    } catch (ServerHandledException ex) {
                        throw ex.InnerException;
                    }
                } catch (FaultException<AuthFault> ex) {
                    throw new UnauthorizedException(currentThread, ex.Message);
                } catch (CommunicationException ex) {
                    throw new BadRequestException(currentThread, ex.Message);
                } catch (Exception ex) {
                    throw new ExceptionThrownException<Exception>(currentThread, ex);
                }
            });
        }
    }

    public class BannerAuthenticateHandler : SecurePathHandler {
        public override IEnumerable<ThreadInfo> VerifyHeaders(ThreadManager TM, NameValueCollection headers, object state) {
            var currentThread = TM.CurrentThread;
            var service = new WebServiceClient();

            string username = headers["Login-Username"];
            string password = headers["Login-Password"];

            if (username == null)
                throw new BadRequestException(currentThread, "Must specify a username.");
            if (password == null)
                throw new BadRequestException(currentThread, "Must specify a password.");

            BannerHandler.VerifyUser(currentThread, username);

            yield return TM.StartNewThread(currentThread, BannerHandler.CatchAuthFailure(currentThread, () => service.Login(username, password)));
            AuthenticationResponse response = TM.GetResult<AuthenticationResponse>(currentThread);
            BannerHandler.LogAuthentication(username, response);

            if (response == null)
                throw new BadRequestException(currentThread, "Login failed: username and/or password is incorrect.");

            var now = DateTime.Now;
            var fallEnd = new DateTime(now.Year, 11, 19);
            fallEnd = fallEnd.AddDays(((int)fallEnd.DayOfWeek + 6) % 7);
            var winterEnd = new DateTime(now.Year, 2, 25);
            winterEnd = winterEnd.AddDays(((int)winterEnd.DayOfWeek + 6) % 7);
            var springEnd = new DateTime(now.Year, 5, 28);
            springEnd = springEnd.AddDays(((int)springEnd.DayOfWeek + 5) % 7);
            var summerEnd = new DateTime(now.Year, 8, 23);
            summerEnd = summerEnd.AddDays(((int)summerEnd.DayOfWeek + 3) % 7);
            int term = now < winterEnd ? now.Year * 100 + 20 :
                now < springEnd ? now.Year * 100 + 30 :
                now < summerEnd ? now.Year * 100 + 40 :
                now < fallEnd ? (now.Year + 1) * 100 + 10 :
                (now.Year + 1) * 100 + 20;

            yield return TM.StartNewThread(currentThread, BannerHandler.CatchAuthFailure(currentThread, () => service.GetTerms(response.Token)));
            var terms = TM.GetResult<int[]>(currentThread);

            yield return TM.Return(currentThread, new BannerAuthResponse(response.Expiration, response.Token, term, terms.Select(t => new Term(t)).ToList()));
        }

        protected override IEnumerable<ThreadInfo> HandleNoPath(ThreadManager TM, Dictionary<string, string> query, object state) {
            var currentThread = TM.CurrentThread;
            yield return TM.Return(currentThread, new JsonResponse((JsonObject)state));
        }
    }

    public class BannerUserHandler : SecurePathHandler {
        public BannerUserHandler() {
            Redirects.Add("data", new BannerRequestDataHandler(new BannerUserDataHandler()));
            Redirects.Add("search", new BannerRequestDataHandler(new BannerUserSearchHandler()));
            Redirects.Add("schedule", new BannerTermRequestDataHandler(new BannerUserScheduleHandler()));
        }

        public override IEnumerable<ThreadInfo> VerifyHeaders(ThreadManager TM, NameValueCollection headers, object state) {
            return BannerHandler.VerifyToken(TM, headers);
        }
    }

    public class BannerUserDataHandler : SecurePathHandler {
        protected override IEnumerable<ThreadInfo> HandleNoPath(ThreadManager TM, Dictionary<string, string> query, object state) {
            var currentThread = TM.CurrentThread;
            var service = new WebServiceClient();
            int requests = 0;

            var requestData = (BannerRequestData)state;
            requests++;
            yield return TM.StartNewThread(currentThread, BannerHandler.CatchAuthFailure(currentThread, () => service.GetUser(requestData.Token, requestData.Id)));
            User user = TM.GetResult<User>(currentThread);
            if (user == null)
                throw new BadRequestException(currentThread, "Cannot find a user with username '{0}'.", requestData.Id);

            UserDataResponse response;
            if (user.Advisor == null) {
                response = new UserDataResponse(user);
            } else {
                requests++;
                yield return TM.StartNewThread(currentThread, BannerHandler.CatchAuthFailure(currentThread, () => service.GetUser(requestData.Token, user.Advisor)));
                var advisor = TM.GetResult<User>(currentThread);
                if (advisor == null)
                    response = new UserDataResponse(user);
                else
                    response = new UserDataResponse(user, advisor);
            }

            BannerHandler.LogRequest(requestData.Token, "Getting user data", requests);
            yield return TM.Return(currentThread, new JsonResponse(response));
        }
    }

    public class BannerUserSearchHandler : SecurePathHandler {
        protected override IEnumerable<ThreadInfo> HandleNoPath(ThreadManager TM, Dictionary<string, string> query, object state) {
            var currentThread = TM.CurrentThread;
            var service = new WebServiceClient();
            var requests = 0;

            var requestData = (BannerRequestData)state;
            var searches = requestData.Id.Split('+', ' ');
            var count = new Dictionary<User, int>();
            bool goodSearch = false;
            foreach (string s in searches) {
                if (s.Length > 1) {
                    goodSearch = true;
                    yield return TM.StartNewThread(currentThread, BannerHandler.CatchAuthFailure(currentThread, () => service.SearchUsers(requestData.Token, s)));
                    var users = TM.GetResult<User[]>(currentThread);
                    requests++;
                    foreach (var user in users) {
                        var match = count.Keys.Where(u => u.Username == user.Username);
                        if (match.Any()) {
                            count[match.First()]++;
                        } else {
                            count[user] = 1;
                        }
                    }
                }
            }

            if (!goodSearch)
                throw new BadRequestException(currentThread, "Search must be at least two characters.");

            BannerHandler.LogRequest(requestData.Token, "Searching users", requests);
            yield return TM.Return(currentThread, new JsonResponse(new UsersResponse(count.OrderBy(kvp => -kvp.Value).Select(kvp => kvp.Key).ToArray())));
        }
    }

    public class BannerUserScheduleHandler : SecurePathHandler {
        protected override IEnumerable<ThreadInfo> HandleNoPath(ThreadManager TM, Dictionary<string, string> query, object state) {
            var currentThread = TM.CurrentThread;
            var service = new WebServiceClient();

            var requestData = (BannerTermRequestData)state;
            yield return TM.StartNewThread(currentThread, BannerHandler.CatchAuthFailure(currentThread, () => service.GetUserEnrollment(requestData.Token, requestData.Term, requestData.Id)));
            var enrollments = TM.GetResult<UserEnrollment[]>(currentThread);
            if (enrollments == null)
                throw new BadRequestException(currentThread, "Cannot find a user with username '{0}'.", requestData.Id);

            yield return TM.StartNewThread(currentThread, BannerHandler.CatchAuthFailure(currentThread, () => service.GetInstructorSchedule(requestData.Token, requestData.Term, requestData.Id)));
            var schedule = TM.GetResult<UserEnrollment[]>(currentThread);
            if (schedule == null)
                throw new BadRequestException(currentThread, "Cannot find a user with username '{0}'.", requestData.Id);

            BannerHandler.LogRequest(requestData.Token, "Getting user schedule", 2);
            yield return TM.Await(currentThread, BannerHandler.GetCourses(TM, query, requestData.Token,
                enrollments.Union(schedule).Select(en => new KeyValuePair<int, int>(en.Term, en.CRN))));
            yield return TM.Return(currentThread, new JsonResponse(TM.GetResult<CoursesResponse>(currentThread)));
        }
    }

    public class BannerCourseHandler : SecurePathHandler {
        public BannerCourseHandler() {
            Redirects.Add("search", new BannerTermRequestDataHandler(new BannerCourseSearchHandler()));
            Redirects.Add("data", new BannerTermRequestDataHandler(new BannerCourseDataHandler()));
        }

        public override IEnumerable<ThreadInfo> VerifyHeaders(ThreadManager TM, NameValueCollection headers, object state) {
            return BannerHandler.VerifyToken(TM, headers);
        }
    }

    public class BannerCourseSearchHandler : SecurePathHandler {
        protected override IEnumerable<ThreadInfo> HandleNoPath(ThreadManager TM, Dictionary<string, string> query, object state) {
            var currentThread = TM.CurrentThread;
            var service = new WebServiceClient();

            var requestData = (BannerTermRequestData)state;
            yield return TM.StartNewThread(currentThread, BannerHandler.CatchAuthFailure(currentThread, () => service.SearchCourses(requestData.Token, requestData.Term, requestData.Id)));
            var courses = TM.GetResult<Course[]>(currentThread);
            if (courses == null)
                throw new BadRequestException(currentThread, "Could not find a course from search '{0}'.", requestData.Id);

            BannerHandler.LogRequest(requestData.Token, "Searching courses", 1);
            yield return TM.Await(currentThread, BannerHandler.GetCourses(TM, query, requestData.Token, courses));
            yield return TM.Return(currentThread, new JsonResponse(TM.GetResult<CoursesResponse>(currentThread)));
        }
    }

    public class BannerCourseDataHandler : SecurePathHandler {
        protected override IEnumerable<ThreadInfo> HandleNoPath(ThreadManager TM, Dictionary<string, string> query, object state) {
            var currentThread = TM.CurrentThread;
            var service = new WebServiceClient();

            var requestData = (BannerTermRequestData)state;
            int crn;
            if (!Int32.TryParse(requestData.Id, out crn))
                throw new BadRequestException(currentThread, "Invalid CRN.");
            yield return TM.StartNewThread(currentThread, BannerHandler.CatchAuthFailure(currentThread, () => service.GetCourse(requestData.Token, requestData.Term, crn)));
            var course = TM.GetResult<Course>(currentThread);
            if (course == null)
                throw new BadRequestException(currentThread, "Could not find a course from term {0} with CRN {1}.", requestData.Term, crn);

            BannerHandler.LogRequest(requestData.Token, "Getting course data", 1);
            yield return TM.Await(currentThread, BannerHandler.GetCourses(TM, query, requestData.Token, new List<Course>() { course }));
            yield return TM.Return(currentThread, new JsonResponse(TM.GetResult<CoursesResponse>(currentThread)));
        }
    }

    public class BannerRoomHandler : SecurePathHandler {
        public BannerRoomHandler() {
            Redirects.Add("schedule", new BannerTermRequestDataHandler(new BannerRoomScheduleHandler()));
        }

        public override IEnumerable<ThreadInfo> VerifyHeaders(ThreadManager TM, NameValueCollection headers, object state) {
            return BannerHandler.VerifyToken(TM, headers);
        }
    }

    public class BannerRoomScheduleHandler : SecurePathHandler {
        protected override IEnumerable<ThreadInfo> HandleNoPath(ThreadManager TM, Dictionary<string, string> query, object state) {
            var currentThread = TM.CurrentThread;
            var service = new WebServiceClient();

            var requestData = (BannerTermRequestData)state;
            yield return TM.StartNewThread(currentThread, BannerHandler.CatchAuthFailure(currentThread, () => service.GetRoomSchedule(requestData.Token, requestData.Term, requestData.Id)));
            var schedule = TM.GetResult<RoomSchedule[]>(currentThread);
            if (schedule == null)
                throw new BadRequestException(currentThread, "Could not find a course that uses room '{0}'.", requestData.Id);

            bool getSchedule = false;
            if (query.ContainsKey("getschedule"))
                Boolean.TryParse(query["getschedule"], out getSchedule);
            query["getschedule"] = "false";

            BannerHandler.LogRequest(requestData.Token, "Getting room schedule", 1);
            yield return TM.Await(currentThread, BannerHandler.GetCourses(TM, query, requestData.Token,
                schedule.Select(sc => new KeyValuePair<int, int>(sc.Term, sc.CRN)).Distinct()));

            var response = TM.GetResult<CoursesResponse>(currentThread);
            if (getSchedule) {
                foreach (var course in response.Courses) {
                    course.Schedule = new List<CourseMeeting>();
                }
                foreach (var time in schedule) {
                    response.Courses.Where(course => course.Term == time.Term && course.CRN == time.CRN).First().
                        Schedule.Add(new CourseMeeting(time, requestData.Id));
                }
            }

            yield return TM.Return(currentThread, new JsonResponse(response));
        }
    }

    public class BannerRequestDataHandler : SecurePathHandler {
        public BannerRequestDataHandler(SecurePathHandler nextPath) {
            UnknownRedirect = nextPath;
        }

        protected override IEnumerable<ThreadInfo> HandleUnknownPath(ThreadManager TM, string path, object state) {
            var currentThread = TM.CurrentThread;
            yield return TM.Return(currentThread, new BannerRequestData((string)state, path));
        }
    }

    public class BannerTermRequestDataHandler : SecurePathHandler {
        public BannerTermRequestDataHandler(SecurePathHandler nextPath) {
            IntRedirect = new BannerTermRequestDataHandler2(nextPath);
        }

        protected override IEnumerable<ThreadInfo> HandleIntPath(ThreadManager TM, int value, object state) {
            var currentThread = TM.CurrentThread;
            yield return TM.Return(currentThread, new BannerTermRequestData((string)state, value));
        }

        private class BannerTermRequestDataHandler2 : SecurePathHandler {
            public BannerTermRequestDataHandler2(SecurePathHandler nextPath) {
                UnknownRedirect = nextPath;
            }

            protected override IEnumerable<ThreadInfo> HandleUnknownPath(ThreadManager TM, string path, object state) {
                var currentThread = TM.CurrentThread;
                var data = (BannerTermRequestData)state;
                data.Id = path;
                yield return TM.Return(currentThread, data);
            }
        }
    }

    public class BannerRequestData {
        public string Token { get; set; }
        public string Id { get; set; }

        public BannerRequestData(string token, string id) {
            Token = token;
            Id = id;
        }
    }

    public class BannerTermRequestData {
        public string Token { get; set; }
        public int Term { get; set; }
        public string Id { get; set; }

        public BannerTermRequestData(string token, int term) {
            Token = token;
            Term = term;
            Id = null;
        }
    }
}