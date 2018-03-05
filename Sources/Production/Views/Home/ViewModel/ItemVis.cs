using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace RoadmapViewer.Views.Home.ViewModel
{
	public class ItemVisTimeLine : ISerializable
	{
		public ItemVisTimeLine(int id, string content, string start)
		{
			this.Id = id;
			this.Content = content;
			this.Start = start;
		}

		public int Id { get; set; }

		public string Content { get; set; }

		public string Start { get; set; }



		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException();
		}
	}
}