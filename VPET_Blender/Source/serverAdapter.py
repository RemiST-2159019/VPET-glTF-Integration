"""
TRACER Scene Distribution Plugin Blender
 
Copyright (c) 2024 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Labs
https://research.animationsinstitut.de/tracer
https://github.com/FilmakademieRnd/TracerSceneDistribution
 
TRACER Scene Distribution Plugin Blender is a development by Filmakademie
Baden-Wuerttemberg, Animationsinstitut R&D Labs in the scope of the EU funded
project MAX-R (101070072) and funding on the own behalf of Filmakademie
Baden-Wuerttemberg.  Former EU projects Dreamspace (610005) and SAUCE (780470)
have inspired the TRACER Scene Distribution Plugin Blender development.
 
The TRACER Scene Distribution Plugin Blender is intended for research and
development purposes only. Commercial use of any kind is not permitted.
 
There is no support by Filmakademie. Since the TRACER Scene Distribution Plugin
Blender is available for free, Filmakademie shall only be liable for intent
and gross negligence; warranty is limited to malice. TRACER Scene Distribution
Plugin Blender may under no circumstances be used for racist, sexual or any
illegal purposes. In all non-commercial productions, scientific publications,
prototypical non-commercial software tools, etc. using the TRACER Scene
Distribution Plugin Blender Filmakademie has to be named as follows: 
"TRACER Scene Distribution Plugin Blender by Filmakademie
Baden-Württemberg, Animationsinstitut (http://research.animationsinstitut.de)".
 
In case a company or individual would like to use the TRACER Scene Distribution
Plugin Blender in a commercial surrounding or for commercial purposes,
software based on these components or  any part thereof, the company/individual
will have to contact Filmakademie (research<at>filmakademie.de) for an
individual license agreement.
 
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
"""

import time 
import threading
import bpy
import struct
import mathutils
import math
from collections import deque
import numpy as np
from .timer import TimerModalOperator

m_pingTimes = deque([0, 0, 0, 0, 0])
pingRTT = 0
## Setup ZMQ thread
def set_up_thread():
    try:
        import zmq
    except Exception as e:
        print('Could not import ZMQ\n' + str(e))
    global vpet, v_prop
    vpet = bpy.context.window_manager.vpet_data
    v_prop = bpy.context.scene.vpet_properties
    # Prepare ZMQ
    vpet.ctx = zmq.Context()

    # Prepare Subscriber
    vpet.socket_s = vpet.ctx.socket(zmq.SUB)
    vpet.socket_s.connect(f'tcp://{v_prop.server_ip}:{v_prop.sync_port}')
    vpet.socket_s.setsockopt_string(zmq.SUBSCRIBE, "")
    vpet.socket_s.setsockopt(zmq.RCVTIMEO,1)
    

    
    bpy.app.timers.register(listener)
    
    # Prepare Distributor
    vpet.socket_d = vpet.ctx.socket(zmq.REP)
    vpet.socket_d.bind(f'tcp://{v_prop.server_ip}:{v_prop.dist_port}')

    # Prepare poller
    vpet.poller = zmq.Poller()
    vpet.poller.register(vpet.socket_d, zmq.POLLIN)    

    bpy.app.timers.register(read_thread)


    
    #bpy.app.timers.register(ping)
    

    bpy.utils.register_class(TimerModalOperator)
    bpy.ops.wm.timer_modal_operator()

    vpet.socket_u = vpet.ctx.socket(zmq.PUB)
    vpet.socket_u.connect(f'tcp://{v_prop.server_ip}:{v_prop.update_sender_port}')

    #set_up_thread_socket_c()

    
def set_up_thread_socket_c():
    global vpet, v_prop
    vpet = bpy.context.window_manager.vpet_data
    v_prop = bpy.context.scene.vpet_properties
    #vpet.ctx = zmq.Context()

    #vpet.socket_c = vpet.ctx.socket(zmq.REQ)
    vpet.socket_c.connect(f'tcp://{v_prop.server_ip}:{v_prop.Command_Module_port}')
   
    ping_thread = threading.Thread(target=ping_thread_function, daemon=True)
    ping_thread.start()
    print("Ping thread started")

## Read requests and send packages
def read_thread():
    global vpet, v_prop
    vpet = bpy.context.window_manager.vpet_data
    v_prop = bpy.context.scene.vpet_properties
    if vpet.socket_d:
        # Get sockets with messages (0: don't wait for msgs)
        sockets = dict(vpet.poller.poll(0))
        # Check if this socket has a message
        if vpet.socket_d in sockets:
            # Receive message
            msg = vpet.socket_d.recv_string()
            #print(msg) # debug
            # Classify message
            if msg == "header":
                print("Header request! Sending...")
                vpet.socket_d.send(vpet.headerByteData)
            elif msg == "nodes":
                print("Nodes request! Sending...")
                vpet.socket_d.send(vpet.nodesByteData)
            elif msg == "objects":
                print("Object request! Sending...")
                vpet.socket_d.send(vpet.geoByteData)
            elif msg == "characters":
                print("Characters request! Sending...")
                if(vpet.charactersByteData != None):
                    vpet.socket_d.send(vpet.charactersByteData)
            elif msg == "textures":
                print("Texture request! Sending...")                
                if(vpet.textureList != None):
                    vpet.socket_d.send(vpet.texturesByteData)
            elif msg == "materials":
                print("Materials request! Sending...")
                if(vpet.materialsByteData != None):
                    vpet.socket_d.send(vpet.materialsByteData)
            elif msg == "curve":
                print("curve request! Sending...")
                if(vpet.curvesByteData != None):
                    vpet.socket_d.send(vpet.curvesByteData)
            else: # sent empty
                vpet.socket_d.send_string("")
    return 0.1 # repeat every .1 second

global last_sync_time
last_sync_time = None 

## process scene updates
def listener():
    global vpet, v_prop, last_sync_time
    vpet = bpy.context.window_manager.vpet_data
    v_prop = bpy.context.scene.vpet_properties
    msg = None
    
    try:
        msg = vpet.socket_s.recv()
    except Exception as e:
        msg = None



    if (msg != None ):  
        clientID = msg[0]
        
        if(vpet.messageType[msg[2]] == "SYNC"):
            current_time = time.time()
            sv_time = msg[1]
            runtime = int(pingRTT * 0.5)
            syncTime = sv_time + runtime
            delta = delta_time(vpet.time, sv_time, TimerModalOperator.my_instance.m_timesteps)
            if delta > 10 or delta>3 and runtime < 8:
                vpet.time = int(round(sv_time)) % TimerModalOperator.my_instance.m_timesteps

        if clientID != vpet.cID:
            msgtime = msg[1]
            type = vpet.messageType[msg[2]]
            #print(type)
  
            start = 3

            while(start < len(msg)):
                
                if(type == "LOCK"):
                    objID = msg[start+1]
                    if 0 < objID <= len(vpet.SceneObjects):
                        lockstate = msg[start+3]
                        vpet.SceneObjects[objID - 1].LockUnlock(lockstate)

                    start = len(msg)
                
                elif(type == "PARAMETERUPDATE"):
                    sceneID = msg[start]
                    objID = msg[start+1]
                    paramID = msg[start+3]
                    length = msg[start+6]
                    parameterData = msg[start+7]

                    if 0 < objID <= len(vpet.SceneObjects) and 0 <= paramID < len(vpet.SceneObjects[objID - 1]._parameterList):
                        param = vpet.SceneObjects[objID - 1]._parameterList[paramID]
                        param.decodeMsg(msg, start+7)
                    
                    start += length

                else:
                    start = len(msg)


                
            
    return 0.01 # repeat every .1 second
                
## Stopping the thread and closing the sockets

def createPingMessage():
    vpet.pingByteMSG= bytearray([])
    vpet.pingByteMSG.extend(struct.pack('B', vpet.cID))
    vpet.pingByteMSG.extend(struct.pack('B', vpet.time))
    vpet.pingByteMSG.extend(struct.pack('B', 3))
    
def ping_thread_function():
    while True:
        ping()
        time.sleep(1)

def ping():
    global vpet, v_prop
    createPingMessage()  # Ensure this updates vpet.pingByteMSG appropriately
    if vpet.socket_c:
        try:
            vpet.socket_c.send(vpet.pingByteMSG)
            vpet.pingStartTime = vpet.time
            msg = vpet.socket_c.recv()
            if msg and msg[0] != vpet.cID:
                DecodePongMessage(msg)
        except Exception as e:
            print(f"Failed to receive pong: {e}")
    
def DecodePongMessage(msg):
    rtt = delta_time(vpet.time, vpet.pingStartTime ,TimerModalOperator.my_instance.m_timesteps)
    pingCount = len(m_pingTimes)
    

    if(pingCount > 4):
        m_pingTimes.popleft()

    m_pingTimes.append(rtt)
    
    rtts = np.array(m_pingTimes)
    rttMax = rtts.max()
    rttSum = rtts.sum()

    if pingCount > 1:
        pingRTT = round((rttSum - rttMax) / (pingCount - 1))
    

def SendParameterUpdate(parameter):
    vpet.ParameterUpdateMSG = bytearray([])
    vpet.ParameterUpdateMSG.extend(struct.pack('B', vpet.cID))
    vpet.ParameterUpdateMSG.extend(struct.pack('B', vpet.time))
    vpet.ParameterUpdateMSG.extend(struct.pack('B', 0))
    vpet.ParameterUpdateMSG.extend(struct.pack('B', vpet.cID))
    vpet.ParameterUpdateMSG.extend(struct.pack('H', parameter._parent._id))
    vpet.ParameterUpdateMSG.extend(struct.pack('H', parameter._id))
    vpet.ParameterUpdateMSG.extend(struct.pack('B', parameter._type))
    length = 7+ parameter._dataSize
    vpet.ParameterUpdateMSG.extend(struct.pack('B', length))
    vpet.ParameterUpdateMSG.extend(parameter.SerializeParameter())

    vpet.socket_u.send(vpet.ParameterUpdateMSG)


def SendLockMSG(sceneObject):
    vpet.ParameterUpdateMSG = bytearray([])
    vpet.ParameterUpdateMSG.extend(struct.pack('B', vpet.cID))
    vpet.ParameterUpdateMSG.extend(struct.pack('B', vpet.time))
    vpet.ParameterUpdateMSG.extend(struct.pack('B', 1))
    vpet.ParameterUpdateMSG.extend(struct.pack('B', vpet.cID))
    vpet.ParameterUpdateMSG.extend(struct.pack('H', sceneObject._id))
    vpet.ParameterUpdateMSG.extend(struct.pack('B', 1))
    vpet.socket_u.send(vpet.ParameterUpdateMSG)

def SendUnlockMSG(sceneObject):
    vpet.ParameterUpdateMSG = bytearray([])
    vpet.ParameterUpdateMSG.extend(struct.pack('B', vpet.cID))
    vpet.ParameterUpdateMSG.extend(struct.pack('B', vpet.time))
    vpet.ParameterUpdateMSG.extend(struct.pack('B', 1))
    vpet.ParameterUpdateMSG.extend(struct.pack('B',vpet.cID))
    vpet.ParameterUpdateMSG.extend(struct.pack('H', sceneObject._id))
    vpet.ParameterUpdateMSG.extend(struct.pack('B', 0))
    vpet.socket_u.send(vpet.ParameterUpdateMSG)
    
def close_socket_d():
    global vpet, v_prop
    vpet = bpy.context.window_manager.vpet_data
    v_prop = bpy.context.scene.vpet_properties
    if bpy.app.timers.is_registered(read_thread):
        print("Stopping thread")
        bpy.app.timers.unregister(read_thread)
        bpy.utils.unregister_class(TimerModalOperator)
    if vpet.socket_d:
        vpet.socket_d.close()
        
def close_socket_s():
    global vpet, v_prop
    vpet = bpy.context.window_manager.vpet_data
    v_prop = bpy.context.scene.vpet_properties
    if bpy.app.timers.is_registered(listener):
        print("Stopping subscription")
        bpy.app.timers.unregister(listener)
    if vpet.socket_s:
        vpet.socket_s.close()

def close_socket_c():
    global vpet, v_prop
    vpet = bpy.context.window_manager.vpet_data
    v_prop = bpy.context.scene.vpet_properties
    if bpy.app.timers.is_registered(createPingMessage):
        print("Stopping createPingMessage")
        bpy.app.timers.unregister(createPingMessage)
    if vpet.socket_c:
        vpet.socket_c.close()

def close_socket_u():
    global vpet, v_prop
    vpet = bpy.context.window_manager.vpet_data
    v_prop = bpy.context.scene.vpet_properties
    if vpet.socket_u:
        vpet.socket_u.close()


def delta_time(startTime, endTime, length):
    def mod(a, b):
        return a % b  
    
    return min(
        mod((startTime - endTime), length),
        mod((endTime - startTime), length)
    )
