using System;
using DSLink.Nodes;
using DSLink.Nodes.Actions;
using DSLink.Request;
using SpotifyAPI.Local;
using ValueType = DSLink.Nodes.ValueType;

namespace DSLink.Spotify
{
	public class SpotifyDSLink : DSLinkContainer
	{
		private readonly SpotifyLocalAPI _localApi;
		private readonly Node _trackNameNode;
		private readonly Node _trackArtistNode;
		private readonly Node _trackAlbumNode;
		private readonly Node _playStateNode;
		private readonly Node _volumeNode;
		private readonly Node _trackTimeNode;
		private readonly Node _trackLengthNode;
		private readonly Node _playPauseNode;
		private readonly Node _nextTrackNode;
		private readonly Node _previousTrackNode;

		public SpotifyDSLink(Configuration configuration) : base(configuration)
		{
			_localApi = new SpotifyLocalAPI();
			_localApi.Connect();

			var status = _localApi.GetStatus();

			_trackNameNode = Responder.SuperRoot.CreateChild("trackName")
									   .SetDisplayName("Track Name")
									   .SetType(ValueType.String)
									   .SetValue(status.Track.TrackResource.Name)
									   .BuildNode();

			_trackArtistNode = Responder.SuperRoot.CreateChild("trackArtist")
									   .SetDisplayName("Track Artist")
									   .SetType(ValueType.String)
									   .SetValue(status.Track.ArtistResource.Name)
									   .BuildNode();

			_trackAlbumNode = Responder.SuperRoot.CreateChild("trackAlbum")
					                   .SetDisplayName("Track Album")
									   .SetType(ValueType.String)
									   .SetValue(status.Track.AlbumResource.Name)
									   .BuildNode();

			_playStateNode = Responder.SuperRoot.CreateChild("playState")
									  .SetDisplayName("Play State")
									  .SetType(ValueType.MakeEnum("Playing", "Stopped"))
									  .SetValue(status.Playing ? "Playing" : "Stopped")
									  .BuildNode();

			_volumeNode = Responder.SuperRoot.CreateChild("volume")
								   .SetDisplayName("Volume")
								   .SetType(ValueType.Number)
								   .SetValue(status.Volume)
								   .BuildNode();

			_trackTimeNode = Responder.SuperRoot.CreateChild("trackTime")
									  .SetDisplayName("Track Time")
									  .SetType(ValueType.Number)
									  .BuildNode();

			_trackLengthNode = Responder.SuperRoot.CreateChild("trackLength")
										.SetDisplayName("Track Length")
										.SetType(ValueType.Number)
			                            .SetValue(status.Track.Length)
										.BuildNode();

			_playPauseNode = Responder.SuperRoot.CreateChild("playPause")
									  .SetDisplayName("Play/Pause")
			                          .SetAction(new ActionHandler(Permission.Write, PlayPauseTrack))
									  .BuildNode();

			_previousTrackNode = Responder.SuperRoot.CreateChild("previousTrack")
										  .SetDisplayName("Previous Track")
										  .SetAction(new ActionHandler(Permission.Write, PreviousTrack))
										  .BuildNode();

			_nextTrackNode = Responder.SuperRoot.CreateChild("nextTrack")
									  .SetDisplayName("Next Track")
									  .SetAction(new ActionHandler(Permission.Write, NextTrack))
									  .BuildNode();

			_localApi.OnTrackChange += TrackChanged;
			_localApi.OnPlayStateChange += PlayStateChanged;
			_localApi.OnVolumeChange += VolumeChanged;
			_localApi.OnTrackTimeChange += TrackTimeChanged;
			_localApi.ListenForEvents = true;
		}

		private void TrackChanged(object sender, TrackChangeEventArgs eventArgs)
		{
			_trackNameNode.Value.Set(eventArgs.NewTrack.TrackResource.Name);
			_trackArtistNode.Value.Set(eventArgs.NewTrack.ArtistResource.Name);
			_trackAlbumNode.Value.Set(eventArgs.NewTrack.AlbumResource.Name);
			_trackLengthNode.Value.Set(eventArgs.NewTrack.Length);
		}

		private void PlayStateChanged(object sender, PlayStateEventArgs eventArgs)
		{
			_playStateNode.Value.Set(eventArgs.Playing ? "Playing" : "Stopped");
		}

		private void VolumeChanged(object sender, VolumeChangeEventArgs eventArgs)
		{
			_volumeNode.Value.Set(eventArgs.NewVolume);
		}

		private void TrackTimeChanged(object sender, TrackTimeChangeEventArgs eventArgs)
		{
			_trackTimeNode.Value.Set((int)eventArgs.TrackTime);
		}

		private void PlayPauseTrack(InvokeRequest request)
		{
			if (_localApi.GetStatus().Playing)
			{
				_localApi.Pause();
			}
			else
			{
				_localApi.Play();
			}
		}

		private void PreviousTrack(InvokeRequest request)
		{
			_localApi.Previous();
			request.Close();
		}

		private void NextTrack(InvokeRequest request)
		{
			_localApi.Skip();
			request.Close();
		}
	}
}
