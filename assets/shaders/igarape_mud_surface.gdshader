shader_type canvas_item;

// The six sheet cells are 1100 x 140; UVs remain in the full texture.
const float CELL_COUNT = 6.0;

void fragment() {
	vec2 local = vec2(fract(UV.x * CELL_COUNT), UV.y);
	float edge = min(local.x, 1.0 - local.x);
	float ragged = 0.008 * sin(local.y * 94.0 + local.x * 53.0);
	float join_alpha = smoothstep(0.0, 0.055, edge + ragged);

	vec4 mud = texture(TEXTURE, UV);
	mud.a *= join_alpha;
	COLOR = mud;
}
