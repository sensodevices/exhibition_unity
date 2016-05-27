using System;

public class UsersFactory {

	const float SCALE = 100f;

	public static void createUser(User user, bool checkClanNameId, string[] param, int start, bool checkPos, bool checkSex, bool checkColors) {
        float px, py;
		int anchor;
        int ind = start;
        string imgId, extra;
        try {
            if (checkClanNameId) {
                user.SetClan(param[ind++]);
                user.SetName(param[ind++]);
                user.SetId(param[ind++].ToInt());
            }
            if (checkSex) {
                user.SetSex(param[ind++].ToInt());
            }
            View view = new View();
			user.AddView(view);
			
			int val = Math.Abs(param[ind++].ToInt());
			for (int k = 0 ; k < val ; ++k) {
				imgId = param[ind++];
				if (imgId == "@") {
					ind += 4;
					//Midlet.sout("set offsets");
					/*founderDyBig = Integer.parseInt(params[ind++]);
					founderDySmall = Integer.parseInt(params[ind++]);
					smotrDyBig = Integer.parseInt(params[ind++]);
					smotrDySmall = Integer.parseInt(params[ind++]);
					isSetDy = true;*/
				} else {
					if (imgId.StartsWith("+")) {
						imgId = imgId.Substring(1);
					}
					px = Coord(param[ind++].ToDouble());
					py = -Coord(param[ind++].ToDouble());
					
					anchor = param[ind++].ToInt();
					extra = param[ind++];
					ImageObject img = user.NewImageObject(imgId);
					img.SetPos(px, py);
					img.SetAnchor(anchor);
					img.layer = -k;
					view.AddImage(img);
					//
					user.LoadImage(img);
				}
			}
						
			if (checkPos) {
				px = Coord(param[ind++].ToDouble());
				//user.SetPos(px*2, 0);
			}
            //проверяем есть ли цвета для телепорта
            /*if (checkColors) {
                //teleportColorMain = Utils.hexToInt(params[ind++]);
                //teleportColorBorder = Utils.hexToInt(params[ind++]);
                //teleportColorLines = Utils.hexToInt(params[ind++]);
                user.teleportSet(params, ind);
            }*/
        } catch(Exception e) {
        }

    }
	
	static float Coord(double value){
		return (float)value/SCALE;
	}
	
}
