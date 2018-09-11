# MujocoUnity

## Reproducing MuJoCo benchmarks in a modern, commercial game /physics engine (Unity + PhysX).

Presented March 19th, 2018 at the AI Summit - Game Developer Conference 2018 - http://schedule.gdconf.com/session/beyond-bots-making-machine-learning-accessible-and-useful/856147

---
## IMPORTANT: Active development has moved to [Marathon-envs](https://github.com/Unity-Technologies/marathon-envs) ##
* This project is now a add-on for Unity ML-Agents Toolkit and is still being maintained and supported. See the new repro here - https://github.com/Unity-Technologies/marathon-envs 

![MujocoUnity](https://github.com/Sohojoe/ml-agents/blob/develop-feature-mujoco-unity/docs/images/MujocoUnityBanner.gif)

----
Legacy readme...

### v0.2 
supports Unity ml-agents ([instructions](Docs/ml_agents.md))

### Trained with ml-agents PPO:

**unity_oai_hopper.xml** 

![Mujoco Hopper 1.5m](Docs/Images/hopper_1.5m_ml_agents_ppo.gif) 1.5m steps 

**unity_dm_walker.xml** 

![Mujoco Hopper 1.5m](Docs/Images/walker_3m_ml_agents_ppo.gif) 3m steps 


### Trained with Baselines DDPG:

**unity_oai_hopper.xml** 

![Mujoco Hopper 300k](https://github.com/Sohojoe/MujocoUnity/blob/master/Docs/Images/hopper_300k.gif) 300k steps ![Mujoco Hopper 300k](https://github.com/Sohojoe/MujocoUnity/blob/master/Docs/Images/hopper_2m.gif) 2m steps 

**unity_dm_walker.xml** 

![Mujoco Hopper 300k](https://github.com/Sohojoe/MujocoUnity/blob/master/Docs/Images/dm_walker_1m.gif) 1m steps ![Mujoco Hopper 300k](https://github.com/Sohojoe/MujocoUnity/blob/master/Docs/Images/dm_walker_3m.gif) 3m steps 


Note: to reproduce you'll need to figure out how to patch OpenAI baselines with Unity. 

### Known Issues:

* oai_humanoid -  is broken. Configurable joint needs updating to support multi-directional joints
* oai_half_cheetah - need to implement geom axis-angle
* dm_xxxx - need to implement class=
* phyx is not tuned properly
* Capsules are slightly too long - NOTE: this can cause collision issues whereby the leg may slightly poke the a foot and trigger collisions


## From earlier version:


**Ant** - Random ![Mujoco Ant random policy](https://github.com/Sohojoe/MujocoUnity/blob/master/Docs/Images/ant-random.gif) Ant - trained with ACKTR ![Mujoco Ant trainged with acktr](https://github.com/Sohojoe/MujocoUnity/blob/master/Docs/Images/ant-acktor.gif)

