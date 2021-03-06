package edu.rosehulman.android.directory.service;

/**
 * Factory class to generate various Client objects.  Two major
 * use implementations include WebClientFactory and MockClientFactory
 *
 */
public interface ClientFactory {
	
	/**
	 * Create an instance of a RestClient object.  See that class's
	 * documentation for details
	 * 
	 * @param host The hostname of the remote server
	 * @param port The port of the remote server
	 * @param path The request path to make
	 * 
	 * @return A new RestClient
	 */
	public RestClient makeRestClient(String host, int port, String path);
	
	/**
	 * Create an instance of a JsonClient object.  See that class's
	 * documentation for details
	 * 
	 * @param host The hostname of the remote server
	 * @param port The port of the remote server
	 * @param path The request path to make
	 * 
	 * @return A new JsonClient
	 */
	public JsonClient makeJsonClient(String host, int port, String path);
	
}
