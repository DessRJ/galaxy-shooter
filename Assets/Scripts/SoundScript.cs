using UnityEngine;

public class SoundScript : MonoBehaviour
{
    public static SoundScript Instance;

    public AudioClip musica;

    public AudioClip disparoNormal;
    public AudioClip disparoAdicional;

    public AudioClip explosion;
    public AudioClip impacto;
    public AudioClip golpeNave;
    public AudioClip powerup;
    public AudioClip cancelacion;
    public AudioClip gameOver;

    public AudioSource audio_disparos;
    public AudioSource audio_explosiones;
    AudioSource audio_musica;

    void Awake()
    {
        Instance = this;
        if (audio_disparos == null) audio_disparos = gameObject.AddComponent<AudioSource>();
        if (audio_explosiones == null) audio_explosiones = gameObject.AddComponent<AudioSource>();
        audio_disparos.playOnAwake = false;
        audio_explosiones.playOnAwake = false;
    }

    void Start()
    {
        audio_musica = gameObject.AddComponent<AudioSource>();
        audio_musica.clip = musica;
        audio_musica.loop = true;
        audio_musica.volume = 0.35f;
        audio_musica.playOnAwake = false;
        if (musica != null) audio_musica.Play();
    }

    void Reproducir(AudioSource fuente, AudioClip clip, float volumen = 1f)
    {
        if (fuente != null && clip != null) fuente.PlayOneShot(clip, volumen);
    }

    public void PlaySonidoDisparo() { Reproducir(audio_disparos, disparoNormal); }
    public void PlaySonidoExplosion() { Reproducir(audio_explosiones, explosion); }

    public void PlayDisparoAdicional() { Reproducir(audio_disparos, disparoAdicional); }
    public void PlayImpacto() { Reproducir(audio_explosiones, impacto, 0.8f); }
    public void PlayGolpeNave() { Reproducir(audio_explosiones, golpeNave); }
    public void PlayPowerup() { Reproducir(audio_explosiones, powerup); }
    public void PlayCancelacion() { Reproducir(audio_explosiones, cancelacion, 0.8f); }
    public void PlayGameOver() { Reproducir(audio_explosiones, gameOver); }

    public void PlayBip(float frecuencia, float duracion)
    {
        const int tasa = 44100;
        int muestras = Mathf.CeilToInt(tasa * duracion);
        float[] datos = new float[muestras];
        for (int i = 0; i < muestras; i++)
        {
            float onda = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * frecuencia * i / tasa));
            datos[i] = onda * 0.25f * (1f - (float)i / muestras);
        }
        AudioClip clip = AudioClip.Create("bip", muestras, 1, tasa, false);
        clip.SetData(datos, 0);
        Reproducir(audio_explosiones, clip);
    }

    public void SubirMusica()
    {
        if (audio_musica != null) audio_musica.volume = 0.35f;
    }

    public void BajarMusica()
    {
        if (audio_musica != null) audio_musica.volume = 0.12f;
    }
}
