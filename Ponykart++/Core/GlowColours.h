namespace Ponykart 
{

	/// Colours available for balloon glow.
	/// 
	/// If you're using these, remember to put the entity into the RENDER_QUEUE_BUBBLE_GLOW render queue!
	enum struct BalloonGlowColour
	{
		red = 0,
		blue = 1,
		yellow = 2,
		green = 3,
		orange = 4,
		magenta = 5,
		purple = 6,
		cyan = 7,
		white = 8
	};
	extern const char* balloonGlowColorNames[]; // Don't forget to update it if BalloonGlowColour changes !

	/// Colours available for flat glow.
	/// 
	/// If you're using these, remember to put the entity into the RENDER_QUEUE_FLAT_GLOW render queue!
	enum struct FlatGlowColour
	{
		red,
		blue,
		yellow,
		green,
		orange,
		magenta,
		purple,
		cyan,
		white,
		black
	};
}
