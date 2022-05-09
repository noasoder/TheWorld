#ifndef GEOMATH_INCLUDED
#define GEOMATH_INCLUDED

// Get point on sphere from long/lat (given in radians)
void LongitudeLatitudeToPoint_float(float2 longLat, out float3 output) {
	float longitude = longLat[0];
	float latitude = longLat[1];

	float y = sin(latitude);
	float r = cos(latitude);
	float x = sin(longitude) * r;
	float z = -cos(longitude) * r;
	output = float3(x,y,z);
}

void UvToLongitudeLatitude_float(float2 uv, out float2 output) {
	float longitude = (uv.x - 0.5) * 2 * 3.141592653589793238462643383279;
	float latitude = (uv.y - 0.5) * 3.141592653589793238462643383279;
	output = float2(longitude, latitude);
}

// Convert point on unit sphere to longitude and latitude
void PointToLongitudeLatitude_float(float3 p, out float2 output) {
	float longitude = atan2(p.x, -p.z);
	float latitude = asin(p.y);
	output = float2(longitude, latitude);
}

void RotateAroundAxis_float(float3 p, float3 axis, float angle, out float3 output) {
	output = p * cos(angle) + cross(axis, p) * sin(angle) + axis * dot(axis, p) * (1 - cos(angle));
}
#endif