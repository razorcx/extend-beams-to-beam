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

				Console.WriteLine("Select extend to beam");

				var extendToBeam = picker.PickObject(Picker.PickObjectEnum.PICK_ONE_PART, "Select beam") as Beam;
				if (extendToBeam == null) return;

				Console.WriteLine("Extend To beam");
				Writeline(extendToBeam);

				var keepGoing = true;
				do
				{
					try
					{
						Console.WriteLine("Select beam to be extended");

						var beamToExtend = picker.PickObject(Picker.PickObjectEnum.PICK_ONE_PART, "Select beam to extend to beam") as Beam;
						if (beamToExtend == null) return;

						var coordSystem = extendToBeam.GetCoordinateSystem();
						var intersectionPlane = new GeometricPlane(coordSystem.Origin, coordSystem.AxisX,
							new Vector(0, 0, 1));

						Operation.DisplayPrompt("Beams are being extended/trimmed to the beam");

						var beamLine = new Line(beamToExtend.StartPoint, beamToExtend.EndPoint);
						var intersectPoint = Intersection.LineToPlane(beamLine, intersectionPlane);
						if (intersectPoint == null) return;

						var startDist = Distance.PointToPoint(intersectPoint, beamToExtend.StartPoint);
						var endDist = Distance.PointToPoint(intersectPoint, beamToExtend.EndPoint);

						if (startDist <= endDist)
							beamToExtend.StartPoint = intersectPoint;
						else
							beamToExtend.EndPoint = intersectPoint;

						if (beamToExtend.Modify())
						{
							if (model.CommitChanges())
							{
								Operation.DisplayPrompt("Beams have been extended/trimmed to the beam");

								Writeline(beamToExtend);
							}
						}
					}
					catch (Exception ex) 
					{
						keepGoing = false;
						Console.WriteLine("Extend command terminated");
						Console.WriteLine("Press any key to continue ...");
						Console.ReadKey();
					}
				} while (keepGoing);
			}
		}

		private static void Writeline(Beam beam)
		{
			Console.WriteLine($"Id: {beam.Identifier.ID}, " +
			                  $"Name: {beam.Name}, " +
			                  $"Profile: {beam.Profile.ProfileString}, " +
			                  $"Material: {beam.Material.MaterialString}");
		}
	}
}
