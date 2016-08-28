#test.py
import win32com.client
import zmq
import numpy
from numpy import array
import sys
import argparse
from gen import updateG
from load import updateL
import load
import ied
import DSSnet_handler as handler
import time
import math

###########
#  Setup  #
###########


engine=win32com.client.Dispatch("OpenDSSEngine.DSS")
engine.Start("0")
engine.Text.Command='clear'
circuit=engine.ActiveCircuit

previous_time=0.0#in seconds used to compute intervals in between calls

def setupCircuit():
    print('starting circuit')
    engine.Text.Command='compile C:\dss\DSSnet\dss\smt\Master.DSS'
    engine.Text.Command='solve'
    #engine.Text.Command='set mode=dynamics number=1'
    engine.Text.Command='solve mode=duty'# number=1'
    engine.Text.Command='solve'
    print('circuit compiled')
 

def get_power_sensor(name): ## mode 1
    DSSMonitors = circuit.Monitors
    #3print(DSSMonitors.name)
    #DSSMonitors.name = name
    #DSSMonitors.reset()
    #print(DSSMonitors.name)
    #engine.Text.Command='solve'
    #print('hi')
    direct('sample')

    DSSMonitors.saveAll()

      
    
    s1 = DSSMonitors.Channel(1)[len(DSSMonitors.Channel(1))-1]
    s2 = DSSMonitors.Channel(3)[len(DSSMonitors.Channel(3))-1]
    s3 = DSSMonitors.Channel(5)[len(DSSMonitors.Channel(5))-1]
    a1 = DSSMonitors.Channel(2)[len(DSSMonitors.Channel(2))-1]
    a2 = DSSMonitors.Channel(4)[len(DSSMonitors.Channel(4))-1]
    a3 = DSSMonitors.Channel(6)[len(DSSMonitors.Channel(6))-1]

    p1 = s1*math.cos(math.radians(a1))
    p2 = s2*math.cos(math.radians(a2))
    p3 = s3*math.cos(math.radians(a3))
    print('S: %s %s %s ' %(s1,s2,s3))
    print('A: %s %s %s ' %(a1,a2,a3))
    print('P: %s %s %s ' %(p1,p2,p3))
    #engine.Text.Command='export monitor mon_wind_gen'
    return ('%s %s %s' %(p1,p2,p3))


def energyStorage(name, p1,p2,p3):
    #print('generator.%s.kW=%s'%(g_name,gen))
    
    if float(p1) < 0:
        engine.Text.Command = 'Storage.%s1.State=Charge'%name
    else:        
        engine.Text.Command = 'Storage.%s1.State=Discharge'%name
    engine.Text.Command = 'Storage.%s1.kW = %s'% (name, str(abs(float(p1))))

    if float(p2) < 0:
        engine.Text.Command = 'Storage.%s2.State=Charge'%name
    else:        
        engine.Text.Command = 'Storage.%s2.State=Discharge'%name
    engine.Text.Command = 'Storage.%s2.kW = %s'% (name ,str(abs(float(p2))))

    if float(p3) < 0:
        engine.Text.Command = 'Storage.%s3.State=Charge'%name
    else:        
        engine.Text.Command = 'Storage.%s3.State=Discharge'%name
    engine.Text.Command = 'Storage.%s3.kW = %s'% (name , str(abs(float(p3))))


    return 'ok' 

def updateTime(dt):
    cmd='set sec=%s' % dt
    engine.Text.Command=cmd
    updateLoads(dt)
    updateGeneration(dt)
    engine.Text.Command='solve'

def exportMonitors():
    #print('hi')
    for m in monitors:
        direct('export monitor %s' % m)
        print('export monitor %s' % m)


monitors = []
monitors.append(mon_bus_650)
monitors.append(mon_bus_634)
monitors.append(mon_bus_675)
monitors.append(mon_bus_646)
monitors.append(mon_bus_611)
monitors.append(mon_bus_652)
monitors.append(mon_bus_680)
monitors.append(mon_ES_1)
monitors.append(mon_ES_2)
monitors.append(mon_ES_3)
monitors.append(mon_wind_gen)

def updateGeneration(name):
	updateG(name,)
	engine.Text.Command='generator.gen.kW='


def updateStorage():
	res = get_power_sensor('mon_wind_gen')
	re = res.split()
	energyStorage('ES',re[0],re[1],re[2])

for x in range(0,10000):
	t=float(x/1000.0)
	result = updateG('gen',t-1.0,0.001)
	
	updateTime(str(t))
	#engine.Text.Command='solve'
	if x % 100 == 0:
		if x % 3500 !=0:

            updateStorage()


exportMonitors()