// Poygon getBounds extension - google-maps-extensions
// http://code.google.com/p/google-maps-extensions/source/browse/google.maps.Polygon.getBounds.js
function getBounds(polygon) {
		var bounds = new google.maps.LatLngBounds();
		var paths = polygon.getPaths();
		var path;

		for (var p = 0; p < paths.getLength() ; p++) {
			path = paths.getAt(p);
			for (var i = 0; i < path.getLength() ; i++) {
				bounds.extend(path.getAt(i));
			}
		}

		return bounds;
	}


// Polygon containsLatLng - method to determine if a latLng is within a polygon

function containsLatLng(poly, latLng) {
	// Exclude points outside of bounds as there is no way they are in the poly

	var lat, lng;


	var bounds = getBounds(poly);

	if (bounds != null && !bounds.contains(latLng)) {
		return false;
	}
	lat = latLng.lat();
	lng = latLng.lng();


	// Raycast point in polygon method
	var inPoly = false;

	var numPaths = poly.getPaths().getLength();
	for (var p = 0; p < numPaths; p++) {
		var path = poly.getPaths().getAt(p);
		var numPoints = path.getLength();
		var j = numPoints - 1;

		for (var i = 0; i < numPoints; i++) {
			var vertex1 = path.getAt(i);
			var vertex2 = path.getAt(j);

			if (vertex1.lng() < lng && vertex2.lng() >= lng || vertex2.lng() < lng && vertex1.lng() >= lng) {
				if (vertex1.lat() + (lng - vertex1.lng()) / (vertex2.lng() - vertex1.lng()) * (vertex2.lat() - vertex1.lat()) < lat) {
					inPoly = !inPoly;
				}
			}

			j = i;
		}
	}

	return inPoly;
}