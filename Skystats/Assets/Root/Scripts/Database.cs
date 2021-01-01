using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MongoDB.Driver;
using MongoDB.Bson;
using System.IO;
using SimpleJSON;
using Newtonsoft.Json;
using System.Text;
using NaughtyAttributes;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;

public class Database : MonoBehaviour
{
	#region Singleton
	public static Database Instance;
	public void OnEnable()
	{
		if (!Instance) Instance = this;
		else Destroy(gameObject);
	}
	#endregion

	MongoClient client = new MongoClient("mongodb+srv://Admin:1kBeNp3T3R@clusterzero.j1n5f.mongodb.net/auction_house?retryWrites=true&w=majority");
	IMongoDatabase database;
	IMongoCollection<BsonDocument> collection;

	private void Start()
	{
		database = client.GetDatabase("auction_house");
		collection = database.GetCollection<BsonDocument>("test_data");
		
		GetData();
	}

	public async void SaveJSON(string path, string jsonData)
	{
		var doc = new BsonDocument { { path, jsonData } };

		await database.DropCollectionAsync("test_data");
		await database.CreateCollectionAsync("test_data");
		collection = database.GetCollection<BsonDocument>("test_data");

		await collection.InsertOneAsync(doc);
	}

	public async void GetData()
	{
		var allTasks = collection.FindAsync(new BsonDocument());
		var taskAwait = await allTasks;

		foreach (var bsonData in taskAwait.ToList())
		{
			var jData = JSON.Parse(bsonData.ToJson());
			var newData = jData["data"].Value;
			newData = newData.Replace("\\\"", "\"");
			var newJData = JSON.Parse(newData);

#if UNITY_EDITOR
			File.WriteAllText(Application.dataPath + "/databaseData.txt", newData);
#endif
		}
	}
}
