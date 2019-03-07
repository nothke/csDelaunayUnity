using System.Collections;
using System.Collections.Generic;

namespace csDelaunay {

	public class Triangle {
        public List<Site> Sites { get; }

        public Triangle(Site a, Site b, Site c) {
			Sites = new List<Site>();
			Sites.Add(a);
			Sites.Add(b);
			Sites.Add(c);
		}

		public void Dispose() {
			Sites.Clear();
		}
	}
}