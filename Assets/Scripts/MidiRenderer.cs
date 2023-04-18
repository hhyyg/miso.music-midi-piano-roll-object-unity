using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using SmfLite;
using SmfLiteExtension;

public class MidiRenderer : MonoBehaviour
{
    public TextAsset MidiSourceFile;
    public float Bpm;
    MidiFileContainer song;
    SmfLiteExtension.MidiNoteSequencer sequencer;

    List<MidiNote> notes;

    PianoGenerator pianoGenerator;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        LoadMidiNotes();
        sequencer = new SmfLiteExtension.MidiNoteSequencer(notes, song.division, Bpm);
        this.pianoGenerator = GameObject.Find("PianoGenerator").GetComponent<PianoGenerator>();
        yield return new WaitForSeconds(1.0f);
        ResetAndPlay(0);
    }

    void LoadMidiNotes()
    {
        song = MidiFileLoader.Load(MidiSourceFile.bytes);
        notes = new List<MidiNote>();
        Debug.Log("division: " + song.division);
        var trackArray = song.tracks.ToArray();

        var draftContainer = new List<MidiNote>();
        var timeLine = 0;

        foreach (var track in trackArray)
        {
            foreach (var eventPair in track)
            {
                bool? eventOn = ((eventPair.midiEvent.status & 0xf0) == 0x90) ? true :
                    ((eventPair.midiEvent.status & 0xf0) == 0x80) ? false : (bool?)null;

                timeLine += eventPair.delta;
                if (eventOn == null)
                {
                    continue;
                }

                if (eventOn.Value)
                {
                    var note = new MidiNote()
                    {
                        Delta = eventPair.delta,
                        Timeline = timeLine,
                        MidiNumber = eventPair.midiEvent.data1,
                        Duration = 0,
                    };
                    Debug.Log($"ON delta:{eventPair.delta} {Pitch.GetPitchByMidiNoteNumber(eventPair.midiEvent.data1)}");

                    draftContainer.Add(note);
                }
                else
                {
                    Debug.Log($"OFF delta:{eventPair.delta} {Pitch.GetPitchByMidiNoteNumber(eventPair.midiEvent.data1)}");

                    // event OFF
                    var foundIndex = draftContainer.FindIndex(x => x.MidiNumber == eventPair.midiEvent.data1);
                    var foundOnEvent = draftContainer.ElementAt(foundIndex);
                    draftContainer.RemoveAt(foundIndex);

                    foundOnEvent.Duration = timeLine - foundOnEvent.Timeline;
                    notes.Add(foundOnEvent);
                }
            }
        }

        notes.Sort((x, y) => x.Timeline.CompareTo(y.Timeline));
    }

    void ResetAndPlay(float startTime)
    {
        DispatchEvents(sequencer.Start(startTime));
    }

    void Update()
    {
        if (sequencer != null && sequencer.Playing)
        {
            DispatchEvents(sequencer.Advance(Time.deltaTime));
        }
    }
    void DispatchEvents(List<MidiNote> notes)
    {
        if (notes != null)
        {
            foreach (var note in notes)
            {
                Debug.Log($"time:{note.Timeline} {Pitch.GetPitchByMidiNoteNumber(note.MidiNumber)} {note.Duration}");
                this.pianoGenerator.InstanciateNote(note, Bpm);
            }
        }
    }
}
