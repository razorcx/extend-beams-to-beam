using System;
using System.Diagnostics;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;
using Tekla.Structures.Model.Operations;

namespace ExtendBeamsToBeam
{
	public class Program
	{
		public static void Main(string[] args)
		{
			try
			{
				ExtendBeamsToBeam.Run();
			}
			catch (Exception ex)
			{
				Trace.WriteLine(ex.InnerException + ex.Message + ex.StackTrace);
			}
		}

		public static class ExtendBeamsToBeam
		{
			public static void Run()
			{
				var model = new Model();
				var picker = new Picker();

				try
				{
					var selected = picker.PickObjects(Picker.PickObjectsEnum.PICK_N_PARTS, "Select beams to extend to beam");
					if (selected.GetSize() < 1) return;

					var pickedBeam = picker.PickObject(Picker.PickObjectEnum.PICK_ONE_PART, "Select beam") as Beam;
					if (pickedBeam == null) return;

					var coordSystem = pickedBeam.GetCoordinateSystem();
					var intersectionPlane = new GeometricPlane(coordSystem.Origin, coordSystem.AxisX,
						new Vector(0, 0, 1));

					Operation.DisplayPrompt("Beams are being extended/trimmed to the beam");

					while (selected.MoveNext())
					{
						var beam = selected.Current as Beam;
						if (beam == null) continue;

						var beamLine = new Line(beam.StartPoint, beam.EndPoint);
						var intersectPoint = Intersection.LineToPlane(beamLine, intersectionPlane);
						if(intersectPoint == null) continue;

						var startDist = Distance.PointToPoint(intersectPoint, beam.StartPoint);
						var endDist = Distance.PointToPoint(intersectPoint, beam.EndPoint);

						if (startDist <= endDist)
							beam.StartPoint = intersectPoint;
						else
							beam.EndPoint = intersectPoint;

						beam.Modify();
					}

					Operation.DisplayPrompt("Beams have been extended/trimmed to the beam");
				}
				catch (Exception ex)
				{
					
				}

				model.CommitChanges();
			}
		}
	}
}
