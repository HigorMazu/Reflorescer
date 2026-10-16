"""Gera loops musicais autorais para a demo de Reflorescer.

Uso: python tools/audio/generate_demo_music.py

Os cinco WAVs são composições instrumentais do projeto: não usam samples,
loops nem material de terceiros. Mantemos este gerador no repositório para
que cada faixa possa ser refeita ou afinada de forma reproduzível.
"""
from __future__ import annotations

import math
import os
import wave

import numpy as np


SAMPLE_RATE = 22050
SECONDS = 32
SAMPLES = SAMPLE_RATE * SECONDS
OUT = os.path.join("assets", "audio", "music")
RNG = np.random.default_rng(20261014)


def envelope(length: int, attack: float = 0.015, release: float = 0.12) -> np.ndarray:
    result = np.ones(length, dtype=np.float64)
    a = max(1, min(length, int(SAMPLE_RATE * attack)))
    r = max(1, min(length, int(SAMPLE_RATE * release)))
    result[:a] = np.linspace(0, 1, a, endpoint=False)
    result[-r:] *= np.linspace(1, 0, r, endpoint=True)
    return result


def oscillator(freq: float, length: int, kind: str = "sine") -> np.ndarray:
    phase = 2 * np.pi * freq * np.arange(length) / SAMPLE_RATE
    if kind == "triangle":
        return 2 / np.pi * np.arcsin(np.sin(phase))
    if kind == "square":
        return np.sign(np.sin(phase)) * 0.55
    return np.sin(phase)


def add_note(track: np.ndarray, start: float, duration: float, midi: int, gain: float,
             kind: str = "sine", attack: float = 0.012, release: float = 0.09) -> None:
    start_i = int(start * SAMPLE_RATE)
    length = min(int(duration * SAMPLE_RATE), len(track) - start_i)
    if length <= 0:
        return
    frequency = 440.0 * 2 ** ((midi - 69) / 12)
    wave_data = oscillator(frequency, length, kind)
    # Harmônicos suaves para evitar o som de oscilador puro.
    if kind == "sine":
        wave_data += 0.17 * oscillator(frequency * 2.01, length)
        wave_data += 0.05 * oscillator(frequency * 3.99, length)
    track[start_i:start_i + length] += gain * wave_data * envelope(length, attack, release)


def add_chord(track: np.ndarray, start: float, duration: float, notes: tuple[int, ...], gain: float) -> None:
    for index, midi in enumerate(notes):
        add_note(track, start, duration, midi, gain * (1.0 if index == 0 else 0.78), "sine", 0.12, 0.34)


def add_delay(track: np.ndarray, delay_seconds: float, gain: float) -> np.ndarray:
    offset = int(delay_seconds * SAMPLE_RATE)
    delayed = track.copy()
    delayed[offset:] += track[:-offset] * gain
    return delayed


def low_noise(length: int, gain: float) -> np.ndarray:
    raw = RNG.normal(0, 1, length)
    alpha = 0.018
    result = np.empty(length)
    state = 0.0
    for i, value in enumerate(raw):
        state += alpha * (value - state)
        result[i] = state
    return result * gain


def forest_layer(track: np.ndarray, density: int, gain: float) -> None:
    for _ in range(density):
        start = float(RNG.uniform(0.6, SECONDS - 0.8))
        note = int(RNG.choice([79, 81, 83, 86, 88, 91]))
        add_note(track, start, float(RNG.uniform(0.06, 0.16)), note, gain, "triangle", 0.003, 0.06)


def title() -> np.ndarray:
    track = np.zeros(SAMPLES)
    progression = [(48, 55, 60), (45, 52, 57), (43, 50, 55), (41, 48, 53)]
    for bar in range(16):
        start = bar * 2.0
        chord = progression[bar % len(progression)]
        add_chord(track, start, 2.2, chord, 0.16)
        for step, note in enumerate((chord[2] + 12, chord[1] + 12, chord[2] + 7, chord[0] + 12)):
            add_note(track, start + step * .5, .42, note, .075, "triangle", .01, .14)
    return add_delay(track, .22, .22)


def sope() -> np.ndarray:
    track = np.zeros(SAMPLES)
    progression = [(45, 52, 57), (41, 48, 52), (48, 55, 60), (43, 50, 55)]
    for bar in range(16):
        start = bar * 2.0
        chord = progression[bar % len(progression)]
        add_chord(track, start, 2.05, chord, .12)
        for beat in range(4):
            add_note(track, start + beat * .5, .36, chord[0] - 12, .09, "triangle", .008, .09)
            add_note(track, start + beat * .5 + .25, .18, chord[2] + 12, .055, "triangle", .004, .06)
    track += low_noise(SAMPLES, .075)
    forest_layer(track, 56, .022)
    return add_delay(track, .17, .16)


def dossel() -> np.ndarray:
    track = np.zeros(SAMPLES)
    progression = [(52, 59, 62), (50, 57, 62), (45, 52, 57), (47, 54, 59)]
    for bar in range(16):
        start = bar * 2.0
        chord = progression[bar % len(progression)]
        add_chord(track, start, 2.2, chord, .1)
        for step in range(8):
            note = chord[(step + bar) % 3] + (12 if step % 2 else 19)
            add_note(track, start + step * .25, .19, note, .055, "triangle", .003, .05)
    forest_layer(track, 72, .018)
    return add_delay(track, .28, .24)


def igarape() -> np.ndarray:
    track = np.zeros(SAMPLES)
    progression = [(38, 45, 50), (36, 43, 48), (41, 48, 53), (36, 43, 48)]
    for bar in range(16):
        start = bar * 2.0
        chord = progression[bar % len(progression)]
        add_chord(track, start, 2.3, chord, .11)
        add_note(track, start, .8, chord[0] - 12, .11, "sine", .03, .22)
        for step in range(3):
            add_note(track, start + .45 + step * .52, .12, chord[2] + 7, .038, "triangle", .004, .07)
    track += low_noise(SAMPLES, .11)
    return add_delay(track, .31, .2)


def covil() -> np.ndarray:
    track = np.zeros(SAMPLES)
    roots = [31, 31, 34, 29, 31, 36, 34, 29]
    for bar in range(16):
        start = bar * 2.0
        root = roots[bar % len(roots)]
        add_chord(track, start, 1.9, (root, root + 7, root + 10), .13)
        for beat in range(4):
            add_note(track, start + beat * .5, .20, root - 12, .16, "square", .003, .10)
            if beat in (1, 3):
                add_note(track, start + beat * .5 + .24, .10, root + 19, .07, "triangle", .002, .045)
        add_note(track, start + 1.5, .18, root + 12, .08, "square", .003, .07)
    track += low_noise(SAMPLES, .035)
    return add_delay(track, .13, .14)


def save(name: str, data: np.ndarray) -> None:
    data -= np.mean(data)
    peak = max(float(np.max(np.abs(data))), .0001)
    data = np.clip(data / peak * 0.66, -1.0, 1.0)
    path = os.path.join(OUT, name)
    pcm = (data * 32767).astype(np.int16)
    with wave.open(path, "wb") as output:
        output.setnchannels(1)
        output.setsampwidth(2)
        output.setframerate(SAMPLE_RATE)
        output.writeframes(pcm.tobytes())
    print(f"{name:28s} {len(data) / SAMPLE_RATE:.1f}s")


def main() -> None:
    os.makedirs(OUT, exist_ok=True)
    save("title_autoral.wav", title())
    save("sope_autoral.wav", sope())
    save("dossel_autoral.wav", dossel())
    save("igarape_autoral.wav", igarape())
    save("covil_autoral.wav", covil())


if __name__ == "__main__":
    main()
