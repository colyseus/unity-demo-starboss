// 
// THIS FILE HAS BEEN GENERATED AUTOMATICALLY
// DO NOT CHANGE IT MANUALLY UNLESS YOU KNOW WHAT YOU'RE DOING
// 
// GENERATED USING @colyseus/schema 1.0.16
// 

using Colyseus.Schema;

public partial class ColyseusRoomState : Schema {
	[Type(0, "map", typeof(MapSchema<ColyseusNetworkedEntity>))]
	public MapSchema<ColyseusNetworkedEntity> networkedEntities = new MapSchema<ColyseusNetworkedEntity>();

	[Type(1, "map", typeof(MapSchema<ColyseusNetworkedUser>))]
	public MapSchema<ColyseusNetworkedUser> networkedUsers = new MapSchema<ColyseusNetworkedUser>();

	[Type(2, "map", typeof(MapSchema<string>), "string")]
	public MapSchema<string> attributes = new MapSchema<string>();
}

