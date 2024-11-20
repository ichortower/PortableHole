# Portable Hole

This mod adds one of my all-time favorite magic items to Stardew Valley: the
Portable Hole. It is a piece of fabric that, when placed on a surface, creates
a portal to a small pocket dimension, a safe extraplanar space where you can
store anything you like. Simply fold up the fabric to close it; then, when you
want to store or retrieve things, place it in a convenient spot to open the
portal once more. It's like having a storage closet, but the door is always
right next to you!

The item can be used almost anywhere, including in the mines and volcano
dungeon. This gives you nearly unfettered access to anything inside, but you
have to stock it ahead of time, and you still have to go in and manipulate
your chests and so on manually, so to me it doesn't feel too cheaty. And
besides, you have to pay for it!


## Translation Credits

- French: [vin163](https://next.nexusmods.com/profile/vin163?gameId=1303)
  (Nexus)


## Requirements

- Stardew Valley 1.6.14+
- SMAPI 4.1.7+

Download, unzip, and enjoy, as usual.


## How to Use

The Portable Hole item, which can be reused infinitely, can be bought from
Krobus's shop in the sewers for 300,000g. Each player may buy only one. Do
not share your reusable inventory item with other players unless you are
prepared to fix problems using debug commands in your SMAPI console (see
Troubleshooting).

Hold the fabric and click to place the portal, just like you would place a
machine. The hole it opens is impassable; interact with it to enter your
personal pocket dimension. Placing the portal somewhere else will automatically
close any previous portal.

NPCs will walk over the portal as if it weren't there and without destroying
it, so do not fear their pathing requirements.

Your portal will remain open until you leave the hole through it, or until you
sleep and start a new day. Other players may enter your hole during this time
(unless you have set DoNotDisturb to true in your config.json; see below). If
other players leave the hole, the portal does not close; only you leaving or
the arrival of a new day will do that.

Do not attempt to trap other players in your hole by leaving (closing) it while
they are inside; the exit will continue to work for them and place them where
they entered. However, if you move the portal somewhere else before they leave,
they will emerge at the new location, a side effect you may find convenient.


## Upgrades

You can upgrade your portable hole by buying two special items. They can be
purchased in any order. As with the portable hole itself, each player may
purchase only one of each. Each one will appear in its respective shop only if
you have already acquired a portable hole. The locations are spoilered here, if
you would like to enjoy the thrill of discovery.

<details>
<summary>The <b>Astral Pylon</b> can be bought from...</summary>
the Shadow Vendor at the Desert Festival, for 300 calico eggs.
</details>

It increases the available space inside your hole.

<details>
<summary>The <b>Dimensional Coupling</b> can be bought from...</summary>
the Qi Gem shop in Mr. Qi's Walnut Room, for 60 Qi Gems.
</details>

It adds a second portal to your hole.

When purchased, these items will appear in your Special Powers tab instead of
entering your inventory. You do not need empty inventory space to purchase them
(but see Known Issues).

After acquiring the coupling, the second portal is managed independently of the
first one, so each one remains open until you use it to leave the hole (other
players will not close your portals, as before). This means you can place one
portal, travel somewhere else, then use the second one to enter the hole and
leave via the first, teleporting back to that location. I'm sure you can find
clever uses for this ability.


## Configuration

Portable Hole reads two config settings from its `config.json`:

- `SecondDoorKey`: (default *LeftShift*) A modifier key. Once you have obtained
the second entrance upgrade, hold this key when clicking to place the second
portal instead of the first.
- `DoNotDisturb`: (true/false, default false) If true, prevents other players
from entering your hole.

These are configurable via Generic Mod Config Menu, which I recommend in
general, but also specifically because GMCM allows you to change Do Not Disturb
during gameplay, which is otherwise not (yet) doable.


## Known Issues

- If you buy either of the upgrade items with a full inventory, a red X message
will appear saying "Inventory Full", even though the purchase completes
successfully and the item takes effect correctly (it never enters your
inventory).
- After acquiring the space upgrade, the next time you enter your hole, the
light effects will appear as they did on the smaller map. It is purely
cosmetic, and on subsequent visits the lights will load correctly.
(This also applies in reverse, if you debug remove your space upgrade, but
that's not conventionally possible)
- The second portal added by the door upgrade differs from the first portal
only by color, which is not great for colorblind players.


## Troubleshooting

If you should somehow lose access to your Portable Hole, it is easy to obtain
another one via the SMAPI console:

```
> debug fin portablehole
```

Any Portable Hole will always open the space tied to the farmer who uses it,
regardless of where it came from; so there is no difference between the item
you buy from Krobus, or the one your farmhand bought, or a "cheated" one
spawned in via other methods. Likewise, the upgrade items apply to the farmer
who obtained them and aren't linked to the fabric object itself.

As soon as a Portable Hole item enters your inventory, you receive a mail flag
indicating that you have obtained one. This flag both enables the upgrade items
to appear in their respective shops and prevents you from buying another
portable hole, so if you share another player's hole item before getting your
own, Krobus won't sell you one. If this happens to you, you can spawn in an
item using the above command and deduct the money manually, or you can run this
command to unset the mail flag:

```
> debug seenmail ichortower.PortableHole_AcquiredHole false
```

You can also use the item spawning and mail flag commands to acquire or
unacquire the upgrade items, but those items never enter your inventory, so
they can't be trivially transferred to other farmers. Just in case, you can
remove the items from your Powers tab by removing the mail flags:

```
> debug seenmail ichortower.PortableHole_SpaceUpgrade false
> debug seenmail ichortower.PortableHole_DoorUpgrade false
```


## Other Questions

### Why are the items so expensive?

From my perspective, having easy at-hand access to a sizeable amount of storage
at the cost of just one inventory space is very powerful, so I think you should
have to pay a good sum for it. Likewise, the second portal lets you do
return-scepter-at-home and similar teleporting shenanigans. If anything, I
think I've made all the items too cheap, considering how useful they are; they
are a steal compared to (for example) Junimo Chests, although I find those very
weak for the price, so maybe that's not the best example.

