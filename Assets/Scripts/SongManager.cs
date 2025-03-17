using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System.IO;
using UnityEngine.Networking;
using System;

public class SongManager : MonoBehaviour
{
    public static SongManager Instance; // Singleton örneği
    public AudioSource audioSource; // Şarkıyı çalmak için kullanılan ses kaynağı
    public Lane[] lanes; // Notaların düşeceği yollar (Lane nesneleri)
    public float songDelayInSeconds; // Şarkının başlamadan önceki gecikme süresi (saniye cinsinden)
    public double marginOfError; // Kullanıcının doğru zamanda tuşa basması için hata toleransı (saniye cinsinden)

    public int inputDelayInMilliseconds; // Giriş gecikmesi (milisaniye cinsinden)

    public string fileLocation; // MIDI dosyasının konumu
    public float noteTime; // Notanın çalınacağı süre
    public float noteSpawnY; // Notanın ekranda doğacağı Y konumu
    public float noteTapY; // Notanın vurulacağı Y konumu
    public float noteDespawnY // Notanın yok olacağı Y konumu
    {
        get
        {
            return noteTapY - (noteSpawnY - noteTapY);
        }
    }

    public static MidiFile midiFile; // MIDI dosyasını tutan değişken

    // Başlangıçta çağrılan fonksiyon
    void Start()
    {
        Instance = this; // Singleton örneğini ayarla
        // Eğer dosya bir web sitesinden yükleniyorsa
        if (Application.streamingAssetsPath.StartsWith("http://") || Application.streamingAssetsPath.StartsWith("https://"))
        {
            StartCoroutine(ReadFromWebsite()); // Web üzerinden MIDI dosyasını oku
        }
        else
        {
            ReadFromFile(); // Yerel dosya sisteminden MIDI dosyasını oku
        }
    }

    // Web üzerinden MIDI dosyasını okuyan fonksiyon
    private IEnumerator ReadFromWebsite()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(Application.streamingAssetsPath + "/" + fileLocation))
        {
            yield return www.SendWebRequest(); // İstek gönder ve yanıtı bekle

            if (www.isNetworkError || www.isHttpError) // Hata kontrolü
            {
                Debug.LogError(www.error); // Hata mesajını yazdır
            }
            else
            {
                byte[] results = www.downloadHandler.data; // Dosya verilerini al
                using (var stream = new MemoryStream(results))
                {
                    midiFile = MidiFile.Read(stream); // MIDI dosyasını oku
                    GetDataFromMidi(); // MIDI dosyasındaki notaları işle
                }
            }
        }
    }

    // Yerel dosya sisteminden MIDI dosyasını okuyan fonksiyon
    private void ReadFromFile()
    {
        midiFile = MidiFile.Read(Application.streamingAssetsPath + "/" + fileLocation); // MIDI dosyasını oku
        GetDataFromMidi(); // MIDI dosyasındaki notaları işle
    }

    // MIDI dosyasındaki notaları işleyen fonksiyon
    public void GetDataFromMidi()
    {
        var notes = midiFile.GetNotes(); // MIDI dosyasından notaları al
        var array = new Melanchall.DryWetMidi.Interaction.Note[notes.Count];
        notes.CopyTo(array, 0); // Notaları diziye kopyala

        foreach (var lane in lanes) lane.SetTimeStamps(array); // Notaları yollarına göre ayarla

         Invoke(nameof(StartSong), songDelayInSeconds); // Belirtilen gecikme süresinden sonra şarkıyı başlat
    }

    // Şarkıyı başlatan fonksiyon
    public void StartSong()
    {
        audioSource.Play(); // Şarkıyı çal
    }

    // Şu an çalan şarkının zamanını saniye cinsinden döndüren fonksiyon
    public static double GetAudioSourceTime()
    {
        return (double)Instance.audioSource.timeSamples / Instance.audioSource.clip.frequency;
    }
}
