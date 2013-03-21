HadoukInput
===========

some nice 2d fighting game code for sanitizing controller input and pattern matching. 

This is written in CSharp, and uses the XNA/Monogame libraries.

## features of this library

* abstracts left/right input into forward/back, depending on which way the character is facing.
* combines keystrokes like "punch + up" into a single keystroke "PunchUp" (just like Smash Brothers input)
* combines left thumbstick and dpad into one input stream.
* some simple networking stuff for sending controller states over a socket
* Keeps track of when buttons are pressed, if they are being held, and when they are released.
* Given an xml file with a list of moves, will parse controller input looking for patterns that match.  For example, this xml file would load a move list with a single Hadouken move:

```xml
<?xml version="1.0"?>
<XnaContent>
	<Asset Type="HadoukInput.MoveListXML">
		<moves>
			<Item type="SPFSettings.MoveXML">
				<name>Hadouken</name>
				<keystrokes>
					<Item Type="string">Down</Item>
					<Item Type="string">Forward</Item>
					<Item Type="string">A</Item>
				</keystrokes>
			</Item>
		</moves>
	</Asset>
</XnaContent>

```

## Some notes

This library puts the thumbsticks in a square gate, and sanitizes all thumbstick and dpad input to up, down, forward, and back.  If you still need to get thumbstick directions, it supports two methods of scrubbing thumbstick input:
* Scrubbed, which has a nice big dead zone and normalizes thumbstick direction
* Powercurve, which curves the thumbstick input for a nice, organic feel:  
https://blogs.msdn.com/b/shawnhar/archive/2007/03/30/massaging-thumbsticks.aspx?Redirected=true

Input is buffered for a split second so it can be combined into single keystrokes, so button+direction presses don't have to be exact

The pattern matching is very forgiving, something like "down, back, forward, A-button" will still throw a hadouken.  It does this because it will also insert extra input like "DownRelease", "ForwardRelease", or might combine forward + A-button into "AForward".  It's better to be lenient anyway, "Be strict in what you send, but generous in what you receive."

If you'd like to see an example of how to use this library, check out the sample at https://github.com/dmanning23/HadoukInputSample

## Future Improvements
* better monogame support.  It should just load up, but I haven't tried it for sure in MonoDevelop yet.
* better networking support: add some methods to tell if the controller state has changed, because otherwise it doesn't need to be sent down the line.  This would save a LOT of bandwidth over just dumping the controller state every time a client's network is updated.
* better button re-mapping support.  There is currently a layer inserted bewteen the raw controller and ControllerWrapper to abstract buttons, but the interface for changing it needs work.

Cheers!