shader_type canvas_item;

uniform float recovery : hint_range(0.0, 1.0) = 0.0;

void fragment() {
	vec2 uv = (UV - 0.5) / 1.03 + 0.5;
	uv += vec2(sin(TIME * 0.16), cos(TIME * 0.12)) * 0.0013;
	vec4 art = texture(TEXTURE, uv);
	float luma = dot(art.rgb, vec3(0.24, 0.68, 0.08));
	vec3 faded = mix(vec3(luma), vec3(luma * 0.94, luma * 0.78, luma * 0.53), 0.42);
	vec3 restored = art.rgb * vec3(0.92, 1.08, 0.96);
	float life = smoothstep(0.0, 1.0, recovery);
	vec3 color = mix(faded * 0.75, restored, life);
	float vignette = 1.0 - 0.38 * smoothstep(0.23, 0.71, distance(UV, vec2(0.5)));
	color *= vignette;
	float mote = pow(max(0.0, sin(UV.x * 98.0 + TIME * 0.4) * sin(UV.y * 72.0 - TIME * 0.3)), 24.0);
	color += vec3(0.22, 0.26, 0.11) * mote * (0.2 + life * 0.8);
	COLOR = vec4(color, art.a);
}
