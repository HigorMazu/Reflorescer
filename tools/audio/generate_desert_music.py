"""Generate Reflorescer's original desert music, including a quiet wind bed.

Run from any directory: python tools/audio/generate_desert_music.py
All tones and noise are synthesized here; no third-party recordings or loops.
"""

from pathlib import Path
import wave

import numpy as np


ROOT = Path(__file__).resolve().parents[2]
OUTPUT = ROOT / "assets/audio/music/deserto_autoral.wav"
RATE = 22050
BAR = 2.5
BARS = 16
SECONDS = BAR * BARS
COUNT = int(RATE * SECONDS)
RNG = np.random.default_rng(20261030)


def midi_frequency(note: int) -> float:
    return 440.0 * 2 ** ((note - 69) / 12)


def note(track: np.ndarray, start: float, duration: float, pitch: int,
         gain: float, timbre: str, attack: float = 0.025) -> None:
    first = int(start * RATE)
    length = min(int(duration * RATE), COUNT - first)
    if length < 2:
        return
    t = np.arange(length) / RATE
    phase = 2 * np.pi * midi_frequency(pitch) * t
    if timbre == "reed":
        tone = np.sin(phase) + .23 * np.sin(2 * phase) + .09 * np.sin(3 * phase)
        tone *= 1 + .025 * np.sin(2 * np.pi * 4.4 * t)
    elif timbre == "bell":
        tone = np.sin(phase) + .25 * np.sin(2.01 * phase) + .07 * np.sin(3.92 * phase)
    else:
        tone = np.sin(phase) + .12 * np.sin(2 * phase)
    envelope = np.minimum(t / attack, 1)
    envelope *= np.minimum((duration - t) / .2, 1).clip(0, 1)
    if timbre == "bell":
        envelope *= np.exp(-2.0 * t / duration)
    track[first:first + length] += gain * tone * envelope


def drum(track: np.ndarray, start: float, gain: float, high: bool = False) -> None:
    first = int(start * RATE)
    length = min(int((.15 if high else .33) * RATE), COUNT - first)
    t = np.arange(length) / RATE
    noise = RNG.normal(0, 1, length)
    if high:
        sound = noise * np.exp(-35 * t)
    else:
        phase = 2 * np.pi * (72 * t - 28 * t * t)
        sound = (np.sin(phase) + .11 * noise) * np.exp(-13 * t)
    track[first:first + length] += gain * sound


def wind() -> np.ndarray:
    frequencies = np.fft.rfftfreq(COUNT, 1 / RATE)
    phase = RNG.uniform(0, 2 * np.pi, len(frequencies))
    band = np.clip((frequencies - 45) / 95, 0, 1)
    band *= np.clip((2500 - frequencies) / 1300, 0, 1)
    spectrum = band * np.exp(1j * phase) / np.sqrt(np.maximum(frequencies, 1))
    noise = np.fft.irfft(spectrum, n=COUNT)
    noise /= np.max(np.abs(noise)) + 1e-9
    t = np.arange(COUNT) / RATE
    gust = .65 + .2 * np.sin(2 * np.pi * 3 * t / SECONDS)
    gust += .1 * np.sin(2 * np.pi * 7 * t / SECONDS + .4)
    return .035 * noise * gust


def compose() -> np.ndarray:
    music = np.zeros(COUNT, dtype=np.float64)
    # D minor with a restrained E-flat accent. Four-bar arc repeats with
    # denser percussion and a higher response in the second half.
    harmony = [(38, 45, 50), (34, 41, 46), (36, 43, 48), (33, 40, 45)]
    melody = [
        (62, 65, 69, 65), (60, 62, 65, 62),
        (60, 63, 67, 63), (57, 60, 62, 60),
    ]
    for bar in range(BARS):
        start = bar * BAR
        root, fifth, octave = harmony[bar % 4]
        note(music, start, 2.65, root, .092, "drone", .2)
        note(music, start, 2.55, fifth, .055, "drone", .3)
        note(music, start, 2.45, octave, .034, "drone", .4)
        drum(music, start, .12)
        drum(music, start + 1.25, .07)
        if bar >= 8:
            drum(music, start + .625, .045, True)
            drum(music, start + 1.875, .05, True)
        phrase = melody[bar % 4]
        for step, pitch in enumerate(phrase):
            if bar < 2 and step % 2:
                continue
            note(music, start + .16 + step * .58, .49, pitch,
                 .066 if bar < 8 else .078, "reed", .025)
        if bar % 4 == 3:
            note(music, start + 1.85, .52, 70 if bar >= 8 else 58,
                 .041, "bell", .006)
    delay = int(.23 * RATE)
    music[delay:] += music[:-delay].copy() * .13
    music += wind()
    # Join the waveform endpoints over a short, inaudible transition.
    blend = int(.035 * RATE)
    weight = np.linspace(0, 1, blend)
    music[-blend:] = music[-blend:] * (1 - weight) + music[0] * weight
    music -= np.mean(music)
    music *= .7 / max(np.max(np.abs(music)), 1e-9)
    return music


def main() -> None:
    OUTPUT.parent.mkdir(parents=True, exist_ok=True)
    pcm = (compose() * 32767).astype("<i2")
    with wave.open(str(OUTPUT), "wb") as output:
        output.setnchannels(1)
        output.setsampwidth(2)
        output.setframerate(RATE)
        output.writeframes(pcm.tobytes())
    print(f"{OUTPUT}: {SECONDS:g}s, {RATE} Hz, mono PCM")


if __name__ == "__main__":
    main()
