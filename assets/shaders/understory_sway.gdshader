shader_type canvas_item;
render_mode unshaded;

// The roots stay planted while the upper leaves move gently.
void vertex() {
	float tip = pow(1.0 - UV.y, 2.0);
	VERTEX.x += sin(TIME * 1.25 + VERTEX.x * 0.012) * tip * 10.0;
}
