using IrrKlang;
using Mogre;
using Ponykart.Core;
using Ponykart.Sound;
using PonykartParsers;

namespace Ponykart.Actors
{
    public class TwiCutlass : Kart
    {
        private readonly float topSpeedKmHour;

        private SoundMain soundMain;
        private ISound idleSound, fullSound;
        private bool idleState;

        public TwiCutlass(ThingBlock block, ThingDefinition def)
            : base(block, def)
        {

            // sounds
            soundMain = LKernel.GetG<SoundMain>();


            idleSound = SoundComponents[0].Sound;
            fullSound = SoundComponents[1].Sound;

            // convert from linear velocity to KPH
            topSpeedKmHour = DefaultMaxSpeed * 3.6f;
            LKernel.GetG<Root>().FrameStarted += FrameStarted;
        }

        /// <summary>
        /// Change the width of the jet engine based on our current speed
        /// </summary>
        bool FrameStarted(FrameEvent evt)
        {
            // crop it to be between 0 and 1
            float relSpeed = _vehicle.CurrentSpeedKmHour / topSpeedKmHour;

            if (relSpeed < 0.5f && !idleState)
            {

                new SoundCrossfader(fullSound, idleSound, 1.65f, 2.0f);

                idleState = true;
            }
            if (relSpeed > 0.5f && idleState)
            {
                new SoundCrossfader(idleSound, fullSound, 1.45f, 2.0f);

                idleState = false;
            }

            return true;
        }

        /// <summary>
        /// Unhook from the event
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

            LKernel.GetG<Root>().FrameStarted -= FrameStarted;

            base.Dispose(disposing);
        }
    }
}
