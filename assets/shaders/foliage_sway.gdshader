shader_type canvas_item;
render_mode unshaded;

// Only the decorative grass fringe moves. Soil and the walking rim stay fixed.
void vertex() {
	float fringe = 1.0 - smoothstep(0.0, 0.15, UV.y);
	VERTEX.x += sin(TIME * 1.4 + VERTEX.x * 0.07) * fringe * 0.65;
}
