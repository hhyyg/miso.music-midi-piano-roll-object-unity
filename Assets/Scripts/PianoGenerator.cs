using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PianoGenerator : MonoBehaviour
{
    public float Speed = 20;
    public GameObject BlackKeyPrefab;
    public GameObject WhiteKeyPrefab;
    public GameObject PianoContainer;
    public GameObject PositionObject;
    private const int _whiteKeyCountFor88 = 52;

    private const int _minMidiNumberFor88 = 21;
    private const int _maxMidiNumberFor88 = 108;
    private static readonly int[] _nextIsBlackKeyNumbers = { 0, 2, 5, 7, 9 };
    private static readonly int[] _blackKeyNumbers = { 1, 3, 6, 8, 10 };

    float whiteKeyWidth = 0;
    Dictionary<int, Vector3> midiNumberPositionDictionary;

    void Start()
    {
        whiteKeyWidth = this.PositionObject.transform.localScale.y / _whiteKeyCountFor88;
        this.midiNumberPositionDictionary = PianoGenerator.CreateMidiNumberPositionDictionary(whiteKeyWidth);

        // centering x
        this.PianoContainer.transform.localPosition = new Vector3(
            this.PositionObject.transform.localPosition.x - (this.midiNumberPositionDictionary.Last().Value.x / 2),
            this.PositionObject.transform.localPosition.y,
            this.PositionObject.transform.localPosition.z
        );
    }

    static Dictionary<int, Vector3> CreateMidiNumberPositionDictionary(float whiteKeyWidth)
    {
        Dictionary<int, Vector3> midiNumberPositionDictionary = new Dictionary<int, Vector3>();

        (float, float, float) nextPosition = (0, 0, 0);
        for (int iNoteNumber = _minMidiNumberFor88; iNoteNumber <= _maxMidiNumberFor88; iNoteNumber++)
        {
            var pitchNumber = iNoteNumber % Pitch.NoteList.Count;
            var isBlackKey = _blackKeyNumbers.Contains(pitchNumber);

            midiNumberPositionDictionary.Add(iNoteNumber, new Vector3(nextPosition.Item1, nextPosition.Item2, nextPosition.Item3));

            var x = nextPosition.Item1 + whiteKeyWidth / 2;
            nextPosition = (x, 0f, 0f);
        }
        return midiNumberPositionDictionary;
    }

    public void InstanciateNote(SmfLiteExtension.MidiNote note, float bpm)
    {
        var pitchNumber = note.MidiNumber % Pitch.NoteList.Count;
        var isBlackKey = _blackKeyNumbers.Contains(pitchNumber);

        GameObject noteObj = UnityEngine.Object.Instantiate<GameObject>(
                   isBlackKey ? this.BlackKeyPrefab : this.WhiteKeyPrefab,
                    new Vector3(0, 0, 0),
                    Quaternion.identity,
                    this.PianoContainer.transform);

        var noteController = noteObj.GetComponent<NoteController>();
        noteController.SetSpeed(this.Speed);

        var heightBase = 60 / bpm * this.Speed;
        var height = heightBase * 0.01f * note.Duration;
        var positionBase = this.midiNumberPositionDictionary[note.MidiNumber];
        noteObj.transform.localPosition = new Vector3(positionBase.x, positionBase.y + (height / 2), positionBase.z);
        noteObj.transform.localScale = new Vector3(whiteKeyWidth, height, whiteKeyWidth);
        noteObj.name = $"{note.Timeline}:{Pitch.GetPitchByMidiNoteNumber(note.MidiNumber)}";
    }
}
