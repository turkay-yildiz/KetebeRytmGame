using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lane : MonoBehaviour
{
    public Melanchall.DryWetMidi.MusicTheory.NoteName noteRestriction; // Bu yolun yalnızca belirli bir nota için çalışmasını sağlar
    public KeyCode input; // Kullanıcının bu yolu tetiklemesi için basması gereken tuş
    public GameObject notePrefab; // Notaların oluşturulacağı prefab
    public List<Note> notes = new List<Note>(); // Aktif notaların listesi
    public List<double> timeStamps = new List<double>(); // Notaların çalınması gereken zamanların listesi

    int spawnIndex = 0; // Hangi notanın oluşturulacağını takip eden indeks
    int inputIndex = 0; // Kullanıcının hangi notaya basacağını takip eden indeks

    // MIDI dosyasındaki notalara göre zaman damgalarını belirler
    public void SetTimeStamps(Melanchall.DryWetMidi.Interaction.Note[] array)
    {
        foreach (var note in array)
        {
            if (note.NoteName == noteRestriction) // Eğer nota, bu yolun belirlenen notasıyla eşleşiyorsa
            {
                // MIDI zaman bilgisini metrik zaman aralığına çevir
                var metricTimeSpan = TimeConverter.ConvertTo<MetricTimeSpan>(note.Time, SongManager.midiFile.GetTempoMap());

                // Notanın çalınma zamanını saniye cinsinden hesapla ve listeye ekle
                timeStamps.Add((double)metricTimeSpan.Minutes * 60f + metricTimeSpan.Seconds + (double)metricTimeSpan.Milliseconds / 1000f);
            }
        }
    }

    // Update metodu, her karede çalışır
    void Update()
    {
        // Eğer yeni bir nota doğurulması gerekiyorsa
        if (spawnIndex < timeStamps.Count)
        {
            // Şu anki ses zamanının, notanın çalınma zamanına denk gelip gelmediğini kontrol et
            if (SongManager.GetAudioSourceTime() >= timeStamps[spawnIndex] - SongManager.Instance.noteTime)
            {
                // Notayı oluştur ve listeye ekle
                var note = Instantiate(notePrefab, transform);
                notes.Add(note.GetComponent<Note>());

                // Notanın atanmış zamanını belirle
                note.GetComponent<Note>().assignedTime = (float)timeStamps[spawnIndex];
                spawnIndex++; // Sonraki notaya geç
            }
        }

        // Eğer kullanıcı doğru notaya basmaya çalışıyorsa
        if (inputIndex < timeStamps.Count)
        {
            double timeStamp = timeStamps[inputIndex]; // Şu anki notanın zamanı
            double marginOfError = SongManager.Instance.marginOfError; // Kabul edilebilir hata payı
            double audioTime = SongManager.GetAudioSourceTime() - (SongManager.Instance.inputDelayInMilliseconds / 1000.0); // Kullanıcının basma zamanı

            // Eğer kullanıcı belirlenen tuşa bastıysa
            if (Input.GetKeyDown(input))
            {
                // Eğer kullanıcı doğru zamanda bastıysa
                if (Math.Abs(audioTime - timeStamp) < marginOfError)
                {
                    Hit(); // Başarıyla vuruldu olarak işaretle
                    print($"Hit on {inputIndex} note"); // Konsola bilgi yazdır
                    Destroy(notes[inputIndex].gameObject); // Notayı sahneden kaldır
                    inputIndex++; // Sonraki notaya geç
                }
                else
                {
                    print($"Hit inaccurate on {inputIndex} note with {Math.Abs(audioTime - timeStamp)} delay"); // Kullanıcı yanlış zamanda bastığında bilgi ver
                }
            }

            // Eğer notanın süresi geçtiyse ve kullanıcı basmadıysa
            if (timeStamp + marginOfError <= audioTime)
            {
                Miss(); // Iskalandı olarak işaretle
                // print($"Missed {inputIndex} note"); // Konsola bilgi yazdır (istersen açabilirsin)
                inputIndex++; // Sonraki notaya geç
            }
        }
    }

    // Kullanıcı başarılı şekilde notaya bastığında çağrılan fonksiyon
    private void Hit()
    {
        ScoreManager.Hit(); // Skor sistemine başarılı vuruşu bildir
    }

    // Kullanıcı bir notayı kaçırdığında çağrılan fonksiyon
    private void Miss()
    {
        ScoreManager.Miss(); // Skor sistemine kaçırılan notayı bildir
    }
}
