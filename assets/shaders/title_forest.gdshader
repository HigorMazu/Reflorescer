shader_type canvas_item;

uniform float elapsed = 0.0;
uniform vec2 pointer = vec2(0.0);

void fragment() {
	vec2 uv = (UV - 0.5) / 1.035 + 0.5;
	uv += pointer * vec2(0.003, 0.002);
	uv += vec2(sin(elapsed * 0.13), cos(elapsed * 0.11)) * 0.0018;
	float water = smoothstep(0.68, 0.79, UV.y) * (1.0 - smoothstep(0.90, 0.97, UV.y));
	water *= smoothstep(0.43, 0.64, UV.x);
	uv.x += sin(uv.y * 290.0 + elapsed * 1.2) * water * 0.0007;
	vec4 art = texture(TEXTURE, uv);
	float shade = mix(0.61, 1.0, smoothstep(0.15, 0.68, UV.x));
	float vignette = 1.0 - 0.23 * smoothstep(0.32, 0.74, distance(UV, vec2(0.55, 0.48)));
	art.rgb *= shade * vignette;
	COLOR = art;
}
