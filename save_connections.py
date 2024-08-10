import mediapipe as mp
from mediapipe.tasks import python
from mediapipe.tasks.python import vision
from mediapipe import solutions
from mediapipe.framework.formats import landmark_pb2
import numpy as np
import cv2 as cv
import time
import matplotlib.pyplot as plt
import socket
import struct
import json
import scipy
BaseOptions = mp.tasks.BaseOptions
FaceLandmarker = mp.tasks.vision.FaceLandmarker
FaceLandmarkerOptions = mp.tasks.vision.FaceLandmarkerOptions
FaceLandmarkerResult = mp.tasks.vision.FaceLandmarkerResult
VisionRunningMode = mp.tasks.vision.RunningMode
model_path = 'face_landmarker.task'

connections = list(frozenset().union(*[mp.solutions.face_mesh.FACEMESH_LIPS, mp.solutions.face_mesh.FACEMESH_LEFT_EYE, mp.solutions.face_mesh.FACEMESH_LEFT_EYEBROW, mp.solutions.face_mesh.FACEMESH_RIGHT_EYE, mp.solutions.face_mesh.FACEMESH_RIGHT_EYEBROW, mp.solutions.face_mesh.FACEMESH_FACE_OVAL, mp.solutions.face_mesh.FACEMESH_TESSELATION]))
# print(connections)
based_connections = []
for connection in connections:
    based_connections.append(connection[0])
    based_connections.append(connection[1])
# print(based_connections)
f = open("connections.json", "w")
f.write(json.dumps(based_connections))
f.close()