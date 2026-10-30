shader_type canvas_item;

void fragment() {
	float edge = min(UV.x, 1.0 - UV.x);
	float ragged = 0.008 * sin(UV.y * 94.0 + UV.x * 53.0);
	float join_alpha = smoothstep(0.0, 0.045, edge + ragged);

	vec4 mud = texture(TEXTURE, UV);
	float value = dot(mud.rgb, vec3(0.299, 0.587, 0.114));
	vec3 clay = vec3(0.34, 0.18, 0.065) * (0.72 + value * 0.68);
	mud.rgb = mix(mud.rgb, clay, 0.68);
	mud.a *= 0.86;
	mud.a *= join_alpha;
	COLOR = mud;
}
