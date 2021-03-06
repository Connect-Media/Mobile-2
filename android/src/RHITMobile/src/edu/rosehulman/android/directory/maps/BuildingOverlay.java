package edu.rosehulman.android.directory.maps;

import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.Paint;
import android.graphics.Paint.Join;
import android.graphics.Paint.Style;
import android.graphics.Path;

import com.google.android.maps.GeoPoint;
import com.google.android.maps.MapView;
import com.google.android.maps.Overlay;
import com.google.android.maps.Projection;

import edu.rosehulman.android.directory.model.Location;
import edu.rosehulman.android.directory.model.MapAreaData;
import edu.rosehulman.android.directory.util.BoundingBox;

/**
 * An overlay for an individual building
 */
public class BuildingOverlay extends Overlay implements Overlay.Snappable {
	
	private Location mapArea;
	
	private BoundingBox bounds;

	private android.graphics.Point pt;
	
	private static Paint paintFill;
	private static Paint paintStroke;
	
	static {
		paintFill = new Paint();
		paintFill.setColor(Color.WHITE);
		paintFill.setAlpha(50);
		paintFill.setStrokeJoin(Join.ROUND);
		paintFill.setStrokeWidth(5.0f);
		paintFill.setStyle(Style.FILL);	
		
		paintStroke = new Paint(paintFill);
		paintStroke.setAlpha(150);
		paintStroke.setStyle(Style.STROKE);		
	}
	
	/**
	 * Creates a new BuildingOverlay
	 * 
	 * @param mapArea The location to initialize from
	 */
	public BuildingOverlay(Location mapArea) {
		this.mapArea = mapArea;
		MapAreaData mapData = mapArea.mapData;
		
		bounds = mapData.getBounds();
	}
	
	/**
	 * Determine the bounding box around this building
	 * 
	 * @return A BoundingBox instance
	 */
	public BoundingBox getBounds() {
		return bounds;
	}
	
	/**
	 * Get the ID of the location this overlay is associated with
	 * 
	 * @return The ID of the location
	 */
	public long getID() {
		return mapArea.id;
	}
	
	/**
	 * Get the location this location is associated with
	 * 
	 * @return The location this location is associated with
	 */
	public Location getLocation() {
		return mapArea;
	}
	
	@Override
	public void draw(Canvas canvas, MapView mapView, boolean shadow) {
		if (shadow) return;
		
		Projection proj = mapView.getProjection();
		MapAreaData mapData = mapArea.mapData;
		
		Path path = new Path();
		pt = proj.toPixels(mapData.corners[0].asGeoPoint(), pt);
		path.moveTo(pt.x, pt.y);
		
		for (int i = 1; i < mapData.corners.length; i++) {
			proj.toPixels(mapData.corners[i].asGeoPoint(), pt);
			path.lineTo(pt.x, pt.y);
		}
		proj.toPixels(mapData.corners[0].asGeoPoint(), pt);
		path.lineTo(pt.x, pt.y);
	
		canvas.drawPath(path, paintFill);		
		canvas.drawPath(path, paintStroke);
	}

	@Override
	public boolean onSnapToItem(int x, int y, android.graphics.Point snapPoint, MapView mapView) {
		Projection proj = mapView.getProjection();
		GeoPoint geoPt = proj.fromPixels(x, y);
		
		if (bounds.contains(geoPt.getLatitudeE6(), geoPt.getLongitudeE6())) {
			proj.toPixels(mapArea.center.asGeoPoint(), snapPoint);
			return true;
		}
		return false;
	}

}
