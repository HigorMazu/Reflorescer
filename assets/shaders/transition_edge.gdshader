shader_type canvas_item;

uniform float top_edge_fade : hint_range(0.0, 1.0) = 1.0;
uniform float top_edge_width : hint_range(0.02, 0.3) = 0.075;
uniform float bottom_edge_fade : hint_range(0.0, 1.0) = 0.0;

// Removes the soft rectangular halo baked into the concept art, keeping the
// irregular roots and rock silhouette against the real level background.
void fragment() {
	vec4 source = texture(TEXTURE, UV) * COLOR;
	source.a *= smoothstep(0.22, 0.78, source.a);
	// Some generated root masses reach the source canvas edge. A slightly
	// irregular top dissolve prevents that edge from reading as a pasted tile.
	float column = floor(UV.x * 48.0);
	float jitter = fract(sin(column * 78.233) * 43758.5453);
	float top_start = 0.025 + jitter * 0.035;
	float organic_top = smoothstep(top_start, top_start + top_edge_width, UV.y);
	source.a *= mix(1.0, organic_top, top_edge_fade);
	float bottom_start = 0.79 + jitter * 0.055;
	float organic_bottom = 1.0 - smoothstep(bottom_start, bottom_start + 0.16, UV.y);
	source.a *= mix(1.0, organic_bottom, bottom_edge_fade);
	COLOR = source;
}
