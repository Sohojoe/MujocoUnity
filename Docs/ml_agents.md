# Using with Unity ml-agents


### Setup
1. Install Unity [ml-agents](https://github.com/Unity-Technologies/ml-agents) (follow their documentation)
2. Add the MujocoBrain (below) to the train_config.yaml file in the python directory
3. In Unity, open hopper or walker from MujocoUnity\Assets\MLA-MujocoUnity\Scenes\
4. Run to see trained brain
5. Set brain to external, build and run from your python path (follow unity ml-agents documentation)



```yaml
MujocoBrain:
    beta: 5.0e-3
    epsilon: 0.20
    gamma: 0.99
    lambd: 0.95
    learning_rate: 1.0e-3
    num_epoch: 3
    time_horizon: 128
    summary_freq: 1000
    use_recurrent: false
    normalize: true
    num_layers: 2
    hidden_units: 64
    batch_size: 2048
    buffer_size: 10240
    max_steps: 3e5
```

