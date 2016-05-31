using System.Collections.Generic;

public class View {

	List<ImageObject> images = new List<ImageObject>();

	public void AddImage (ImageObject value) {
		images.Add(value);
	}
	
}
