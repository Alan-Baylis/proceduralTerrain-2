using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class GUIManager : MonoBehaviour {
	public Text guiTextMode;
	public Slider sizeSlider;
	public TexturePainter painter;
	public Button[] modes;

	public void onStart(){
		painter.SetBrushSize (sizeSlider.value * 10);
		painter.setVisualization (0);
		modes [0].Select();
	}

	public void SetBrushMode(int newMode){
		Painter_BrushMode brushMode =newMode==0? Painter_BrushMode.DECAL:Painter_BrushMode.PAINT; //Cant set enums for buttons :(
		string colorText=brushMode==Painter_BrushMode.PAINT?"orange":"purple";	
		guiTextMode.text="<b>Mode:</b><color="+colorText+">"+brushMode.ToString()+"</color>";
	}

	public void UpdateSizeSlider(){
		painter.SetBrushSize (sizeSlider.value * 10);
	}

	public void VisualizeTerrain(){
		painter.setVisualization (0);
	}

	public void PaintHumidity(){
		painter.setVisualization (1);
	}

	public void PaintHeight(){
		painter.setVisualization (2);
	}

	public void PaintTemperature(){
		painter.setVisualization (3);
	}

	public void RandomizeHumidityTemperature(){
		painter.randomizeHumidityAndTemperature();
	}
		
}
