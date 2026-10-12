"""Gera os 12 efeitos sonoros PLACEHOLDER esperados pelo código (ROADMAP_DEV C11-T2).

Sons sintetizados aqui (formas de onda + ruído filtrado), 100% do projeto, sem licença de
terceiros. Servem pra destravar o áudio e o QA; cada arquivo pode ser trocado pelo som final
sem mexer no código, desde que mantenha o mesmo nome em assets/audio/sfx/.

Uso (na raiz do projeto Godot):  python tools/audio/generate_placeholder_sfx.py
Requer numpy. Saída: WAV PCM 16 bits, mono, 44,1 kHz, pico normalizado em -3 dBFS.
"""
import os
import wave

import numpy as np

SR = 44100
OUT = os.path.join("assets", "audio", "sfx")
rng = np.random.default_rng(15)  # semente fixa: o mesmo script gera sempre os mesmos arquivos


def t(dur):
    return np.arange(int(SR * dur)) / SR


def env(n, attack=0.005, release=0.08, curve=2.0):
    """Envelope ataque-rápido + decaimento exponencial-ish."""
    a = max(1, int(SR * attack))
    e = np.ones(n)
    e[:a] = np.linspace(0, 1, a)
    tail = np.linspace(1, 0, n - a) ** curve
    e[a:] = tail
    if release:
        r = min(n, int(SR * release))
        e[-r:] *= np.linspace(1, 0, r)
    return e


def tone(freq_start, freq_end, dur, wave_kind="sine"):
    """Tom com glissando exponencial de freq_start até freq_end."""
    tt = t(dur)
    f = freq_start * (freq_end / freq_start) ** (tt / dur)
    phase = 2 * np.pi * np.cumsum(f) / SR
    if wave_kind == "square":
        return np.sign(np.sin(phase)) * 0.6
    if wave_kind == "tri":
        return 2 / np.pi * np.arcsin(np.sin(phase))
    return np.sin(phase)


def noise(dur):
    return rng.uniform(-1, 1, int(SR * dur))


def lowpass(x, cutoff):
    """Passa-baixas de 1 polo; cutoff pode ser escalar ou array (varredura)."""
    cutoff = np.broadcast_to(np.asarray(cutoff, dtype=float), x.shape)
    alpha = 1 - np.exp(-2 * np.pi * cutoff / SR)
    y = np.empty_like(x)
    acc = 0.0
    for i in range(len(x)):
        acc += alpha[i] * (x[i] - acc)
        y[i] = acc
    return y


def bandpass(x, low, high):
    return lowpass(x, high) - lowpass(x, low)


def mix(*parts):
    n = max(len(p) for p in parts)
    out = np.zeros(n)
    for p in parts:
        out[: len(p)] += p
    return out


def pad(x, start):
    return np.concatenate([np.zeros(int(SR * start)), x])


def chime(freq, dur, start=0.0, gain=1.0):
    """Sino suave: fundamental + harmônico, decaimento longo."""
    tt = t(dur)
    s = np.sin(2 * np.pi * freq * tt) + 0.35 * np.sin(2 * np.pi * freq * 2.01 * tt)
    return pad(s * env(len(tt), 0.004, 0.05, 3.0) * gain, start)


def save(name, x):
    x = x - np.mean(x)
    peak = np.max(np.abs(x)) or 1.0
    x = x / peak * 10 ** (-3 / 20)  # -3 dBFS
    fade = min(len(x), int(SR * 0.004))
    x[-fade:] *= np.linspace(1, 0, fade)  # sem clique no fim
    data = (x * 32767).astype(np.int16)
    path = os.path.join(OUT, name)
    with wave.open(path, "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(SR)
        w.writeframes(data.tobytes())
    print(f"{name:24s} {len(x) / SR:.2f} s")


def main():
    os.makedirs(OUT, exist_ok=True)

    # Kairo
    d = 0.18  # estocada: sopro curto que fecha + brilho de lâmina de grama
    whoosh = bandpass(noise(d), np.linspace(2500, 600, int(SR * d)), np.linspace(7000, 2500, int(SR * d)))
    save("player_attack.wav", mix(whoosh * env(int(SR * d), 0.01, 0.03, 1.5), 0.25 * tone(1800, 1200, d, "tri") * env(int(SR * d), 0.002, 0.03, 3)))
    d = 0.14
    save("player_jump.wav", tone(320, 640, d, "square") * env(int(SR * d), 0.003, 0.04, 1.5) * 0.5)
    d = 0.28
    thud = lowpass(noise(d), 900) * env(int(SR * d), 0.002, 0.05, 3)
    save("player_hurt.wav", mix(tone(520, 180, d, "square") * env(int(SR * d), 0.002, 0.06, 1.2) * 0.5, thud))
    d = 0.22
    save("player_dash.wav", bandpass(noise(d), np.linspace(400, 1500, int(SR * d)), np.linspace(1500, 6000, int(SR * d))) * env(int(SR * d), 0.02, 0.06, 1.2))

    # Espada de Grama: brota (sobe) / recolhe (desce)
    save("sword_on.wav", mix(chime(660, 0.25), chime(880, 0.25, 0.06, 0.8), chime(1320, 0.3, 0.12, 0.6),
                             0.15 * bandpass(noise(0.3), 3000, 9000) * env(int(SR * 0.3), 0.05, 0.1, 2)))
    save("sword_off.wav", mix(chime(1100, 0.2, 0, 0.7), chime(740, 0.25, 0.07, 0.6),
                              0.2 * lowpass(noise(0.25), 1500) * env(int(SR * 0.25), 0.01, 0.08, 2)))

    # Inimigos
    d = 0.12
    save("enemy_hit.wav", mix(bandpass(noise(d), 800, 5000) * env(int(SR * d), 0.001, 0.02, 4),
                              tone(220, 90, d) * env(int(SR * d), 0.001, 0.03, 2)))
    d = 0.45
    save("enemy_death.wav", mix(tone(400, 70, d, "square") * env(int(SR * d), 0.002, 0.1, 1.3) * 0.45,
                                lowpass(noise(d), np.linspace(3000, 200, int(SR * d))) * env(int(SR * d), 0.002, 0.1, 2) * 0.6))

    # Mundo
    save("checkpoint_activate.wav", mix(chime(523.25, 0.6), chime(659.25, 0.6, 0.09), chime(783.99, 0.8, 0.18)))
    notes = [392.0, 493.88, 587.33, 783.99, 987.77]  # Sol maior subindo: a mata volta a crescer
    shimmer = 0.12 * bandpass(noise(1.3), 4000, 10000) * env(int(SR * 1.3), 0.3, 0.3, 1.5)
    save("restoration.wav", mix(*[chime(f, 0.9, i * 0.11, 1 - i * 0.1) for i, f in enumerate(notes)], shimmer))

    # Korrag
    d = 0.7
    boom = tone(90, 35, d) * env(int(SR * d), 0.002, 0.15, 1.6)
    rumble = lowpass(noise(d), np.linspace(600, 120, int(SR * d))) * env(int(SR * d), 0.005, 0.2, 1.5) * 0.8
    save("boss_stomp.wav", mix(boom, rumble))
    d = 1.2
    roar = lowpass(tone(180, 60, d, "square") * 0.5 + 0.5 * noise(d), 1200) * env(int(SR * d), 0.03, 0.3, 1.2)
    resolve = mix(chime(392.0, 1.0, 1.0, 0.5), chime(523.25, 1.0, 1.15, 0.5), chime(659.25, 1.2, 1.3, 0.5))
    save("boss_defeat.wav", mix(roar, pad(boom * 0.8, 0.9), resolve))


if __name__ == "__main__":
    main()
