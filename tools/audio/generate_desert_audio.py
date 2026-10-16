"""Create original synthesized wind ambience and snake hiss for the desert review."""

from pathlib import Path
import wave

import numpy as np


ROOT = Path(__file__).resolve().parents[2]
RATE = 22050
RNG = np.random.default_rng(20261015)


def write_wav(path: Path, signal: np.ndarray) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    signal = np.clip(signal, -.98, .98)
    pcm = (signal * 32767).astype("<i2")
    with wave.open(str(path), "wb") as output:
        output.setnchannels(1)
        output.setsampwidth(2)
        output.setframerate(RATE)
        output.writeframes(pcm.tobytes())


def periodic_noise(seconds: int, low: float, high: float) -> np.ndarray:
    count = RATE * seconds
    frequencies = np.fft.rfftfreq(count, 1 / RATE)
    phases = RNG.uniform(0, 2 * np.pi, len(frequencies))
    taper = np.clip((frequencies - low) / max(low, 1), 0, 1)
    taper *= np.clip((high - frequencies) / max(high * .35, 1), 0, 1)
    spectrum = taper * np.exp(1j * phases) / np.sqrt(np.maximum(frequencies, 1))
    sound = np.fft.irfft(spectrum, n=count)
    return sound / (np.max(np.abs(sound)) + 1e-9)


def desert_wind() -> np.ndarray:
    seconds = 12
    t = np.arange(RATE * seconds) / RATE
    low = periodic_noise(seconds, 35, 1800)
    grit = periodic_noise(seconds, 1300, 5700)
    gust = .58 + .24 * np.sin(2 * np.pi * t / seconds * 2 + .3) + .12 * np.sin(2 * np.pi * t / seconds * 5)
    return .18 * gust * low + .027 * (gust + .2) * grit


def snake_hiss() -> np.ndarray:
    count = round(RATE * .85)
    noise = RNG.normal(0, 1, count)
    spectrum = np.fft.rfft(noise)
    frequencies = np.fft.rfftfreq(count, 1 / RATE)
    spectrum *= np.clip((frequencies - 700) / 1300, 0, 1) * np.clip((9000 - frequencies) / 2500, 0, 1)
    hiss = np.fft.irfft(spectrum, n=count)
    hiss /= np.max(np.abs(hiss)) + 1e-9
    t = np.arange(count) / RATE
    envelope = np.minimum(t / .08, 1) * np.minimum((.85 - t) / .18, 1)
    return .32 * hiss * np.clip(envelope, 0, 1)


if __name__ == "__main__":
    write_wav(ROOT / "assets/audio/ambience/desert_wind.wav", desert_wind())
    write_wav(ROOT / "assets/audio/sfx/snake_hiss.wav", snake_hiss())
    print("Generated original desert wind loop and snake hiss.")
