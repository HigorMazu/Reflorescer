shader_type canvas_item;

void fragment() {
	float edge = min(UV.x, 1.0 - UV.x);
	float ragged = 0.01 * sin(UV.y * 83.0 + UV.x * 31.0);
	float fade = smoothstep(0.0, 0.065, edge + ragged);
	COLOR.a *= fade;
}
