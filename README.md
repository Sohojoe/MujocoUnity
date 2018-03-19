# MujocoUnity

## Reproducing MuJoCo benchmarks in a modern, commercial game /physics engine (Unity + PhysX).

Presented March 19th, 2018 at the AI Summit - Game Developer Conference 2018 - http://schedule.gdconf.com/session/beyond-bots-making-machine-learning-accessible-and-useful/856147

### Trained with Baselines DDPG:

**unity_oai_hopper.xml** 300k steps ![Mujoco Hopper 300k](https://github.com/Sohojoe/MujocoUnity/blob/master/Docs/Images/hopper_300k.gif) 2m steps ![Mujoco Hopper 300k](https://github.com/Sohojoe/MujocoUnity/blob/master/Docs/Images/hopper_2m.gif)

**unity_dm_walker.xml** 1m steps ![Mujoco Hopper 300k](https://github.com/Sohojoe/MujocoUnity/blob/master/Docs/Images/dm_walker_1m.gif) 3m steps ![Mujoco Hopper 300k](https://github.com/Sohojoe/MujocoUnity/blob/master/Docs/Images/dm_walker_3m.gif)


Note: to reproduce you'll need to figure out how to patch OpenAI baselines with Unity. 

### Known Issues:

* oai_humanoid -  is broken. Configurable joint needs updating to support multi-directional joints
* oai_half_cheetah - need to implement geom axis-angle
* dm_xxxx - need to implement class=
* phyx is not tuned properly
* Capsules are slightly too long - NOTE: this can cause collision issues whereby the leg may slightly poke the a foot and trigger collisions


## From earlier version:


**Ant** - Random ![Mujoco Ant random policy](https://github.com/Sohojoe/MujocoUnity/blob/master/Docs/Images/ant-random.gif) Ant - trained with ACKTR ![Mujoco Ant trainged with acktr](https://github.com/Sohojoe/MujocoUnity/blob/master/Docs/Images/ant-acktor.gif)

