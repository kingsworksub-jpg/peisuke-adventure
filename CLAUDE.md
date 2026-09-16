# Peisuke's Adventure (ペイスケの冒険) — Project Notes

## Speech bubble font size

All speech-bubble / dialogue text (the bubble shown when Peisuke examines an
object, and any line Peisuke himself says) must use one consistent, fixed
font size — never Unity's `resizeTextForBestFit`. Auto-fit made short
messages (e.g. "あっちっち！") render much larger than long ones (e.g.
"まだ外にいく準備が出来ていない…"), which looked inconsistent.

- Font size is defined once as `SpeechBubbleEffect.BubbleFontSize` (currently
  60) in `Assets/Scripts/SpeechBubbleEffect.cs`.
- All bubble text (fireplace, nightstand, door, and any future interactable)
  goes through `SpeechBubbleEffect.Say(string)`, so it automatically gets
  this size — don't set `fontSize` or `resizeTextForBestFit` anywhere else
  for this bubble.
- If a new message turns out too long to fit the bubble at this size,
  enlarge the bubble's texture/core-rect constants in
  `SpeechBubbleEffect.cs` rather than re-enabling auto-fit or shrinking the
  font for that one message.
- This does not apply to other UI text (buttons, HP/level numbers, status
  panel) — only the speech-bubble/dialogue system.
