using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SmfLiteExtension
{
    public class MidiNoteSequencer
    {
        readonly IEnumerator<MidiNote> enumerator;
        readonly float pulsePerSecond;
        bool playing;
        float timeLineToNext;
        float timeLine;

        public MidiNoteSequencer(List<MidiNote> notes, int ppqn, float bpm)
        {
            pulsePerSecond = bpm / 60.0f * ppqn;
            enumerator = notes.GetEnumerator();
        }

        public bool Playing
        {
            get { return playing; }
        }

        public List<MidiNote> Start(float startTime = 0.0f)
        {
            if (enumerator.MoveNext())
            {
                timeLineToNext = enumerator.Current.Timeline;
                playing = true;
                return Advance(startTime);
            }
            else
            {
                playing = false;
                return null;
            }
        }

        public List<MidiNote> Advance(float deltaTime)
        {
            if (!playing)
            {
                return null;
            }

            timeLine += pulsePerSecond * deltaTime;

            if (timeLine < timeLineToNext)
            {
                return null;
            }

            var messages = new List<MidiNote>();

            while (timeLine >= timeLineToNext)
            {
                var current = enumerator.Current;
                messages.Add(current);
                if (!enumerator.MoveNext())
                {
                    playing = false;
                    break;
                }

                timeLineToNext = enumerator.Current.Timeline;
            }

            return messages;
        }
    }
}