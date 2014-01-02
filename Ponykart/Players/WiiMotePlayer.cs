using System;
using Ponykart.Levels;
using WiimoteLib;
using Ponykart.Core;
using System.Windows.Forms;

namespace Ponykart.Players
{
	public class WiiMotePlayer : Player
	{
		private  Wiimote _wiimote;

		public WiiMotePlayer(LevelChangedEventArgs eventArgs, int id)
			: base(eventArgs, id, false)
		{
			while (true)
			{
				try
				{
					_wiimote = new Wiimote();
					_wiimote.Connect();
					break;
				}
				catch (WiimoteNotFoundException)
				{
					MessageBox.Show("Please ensure a wiimote is paired to your computer via Bluetooth, then press OK to try again");
				}

			}
			_wiimote.SetLEDs(id + 1);
		}

		public override bool IsControlEnabled
		{
			get { return base.IsControlEnabled; }
			set
			{
				if (base.IsControlEnabled != value)
				{
					if (value)
						_wiimote.WiimoteChanged += wiimote_WiimoteChanged;
					else
						_wiimote.WiimoteChanged -= wiimote_WiimoteChanged;
				}
				base.IsControlEnabled = value;
			}
		}

		private void wiimote_WiimoteChanged(object sender, WiimoteChangedEventArgs e)
		{
			if (Kart != null)
			{
				Kart.Acceleration = e.WiimoteState.ButtonState.A ? 1 : 0;
				Kart.Acceleration -= e.WiimoteState.ButtonState.B ? 1 : 0;
				Kart.TurnMultiplier = -e.WiimoteState.NunchukState.Joystick.X * 2;
			}
		}

		protected override void UseItem()
		{
			throw new NotImplementedException();
		}

		public override void Detach()
		{
			_wiimote.Dispose();
			base.Detach();
		}
	}
}
