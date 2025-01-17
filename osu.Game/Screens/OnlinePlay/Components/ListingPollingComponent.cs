// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Lounge.Components;

namespace osu.Game.Screens.OnlinePlay.Components
{
    /// <summary>
    /// A <see cref="RoomPollingComponent"/> that polls for the lounge listing.
    /// </summary>
    public partial class ListingPollingComponent : RoomPollingComponent
    {
        public IBindable<bool> InitialRoomsReceived => initialRoomsReceived;
        private readonly Bindable<bool> initialRoomsReceived = new Bindable<bool>();

        public readonly Bindable<FilterCriteria?> Filter = new Bindable<FilterCriteria?>();

        [BackgroundDependencyLoader]
        private void load()
        {
            Filter.BindValueChanged(_ =>
            {
                RoomManager.ClearRooms();
                initialRoomsReceived.Value = false;

                if (IsLoaded)
                    PollImmediately();
            });
        }

        private GetRoomsRequest? lastPollRequest;

        protected override Task Poll()
        {
            if (!API.IsLoggedIn)
                return base.Poll();

            if (Filter.Value == null)
                return base.Poll();

            var tcs = new TaskCompletionSource<bool>();

            lastPollRequest?.Cancel();

            var req = new GetRoomsRequest(Filter.Value);

            req.Success += result =>
            {
                result = result.Where(r => r.Category != RoomCategory.DailyChallenge).ToList();

                foreach (var existing in RoomManager.Rooms.ToArray())
                {
                    if (result.All(r => r.RoomID != existing.RoomID))
                        RoomManager.RemoveRoom(existing);
                }

                foreach (var incoming in result)
                    RoomManager.AddOrUpdateRoom(incoming);

                initialRoomsReceived.Value = true;
                tcs.SetResult(true);
            };

            req.Failure += _ => tcs.SetResult(false);

            API.Queue(req);

            lastPollRequest = req;
            return tcs.Task;
        }
    }
}
