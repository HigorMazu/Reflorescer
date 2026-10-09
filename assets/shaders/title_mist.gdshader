shader_type canvas_item;

uniform float elapsed = 0.0;

float hash(vec2 p) { return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453); }
float noise(vec2 p) {
	vec2 cell = floor(p);
	vec2 f = fract(p);
	f = f * f * (3.0 - 2.0 * f);
	return mix(mix(hash(cell), hash(cell + vec2(1.0, 0.0)), f.x),
		mix(hash(cell + vec2(0.0, 1.0)), hash(cell + vec2(1.0)), f.x), f.y);
}
void fragment() {
	vec2 p = UV * vec2(5.0, 11.0) + vec2(elapsed * 0.028, elapsed * 0.008);
	float mist = noise(p) * 0.65 + noise(p * 2.1) * 0.35;
	float band = exp(-pow((UV.y - 0.73) * 5.0, 2.0));
	float right = smoothstep(0.28, 0.72, UV.x);
	float ray = pow(max(0.0, sin((UV.x + UV.y * 0.35) * 23.0 + elapsed * 0.07)), 12.0);
	float alpha = mist * band * right * 0.10 + ray * right * (1.0 - UV.y) * 0.022;
	COLOR = vec4(mix(vec3(0.48, 0.68, 0.61), vec3(1.0, 0.86, 0.56), ray), alpha);
}
