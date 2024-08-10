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

addr = ("127.0.0.1", 3030)
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

def draw_landmarks_on_image(rgb_image, detection_result):
  face_landmarks_list = detection_result.face_landmarks
  annotated_image = np.copy(rgb_image)

  # Loop through the detected faces to visualize.
  for idx in range(len(face_landmarks_list)):
    face_landmarks = face_landmarks_list[idx]

    # Draw the face landmarks.
    face_landmarks_proto = landmark_pb2.NormalizedLandmarkList()
    face_landmarks_proto.landmark.extend([
      landmark_pb2.NormalizedLandmark(x=landmark.x, y=landmark.y, z=landmark.z) for landmark in face_landmarks
    ])

    solutions.drawing_utils.draw_landmarks(
        image=annotated_image,
        landmark_list=face_landmarks_proto,
        connections=mp.solutions.face_mesh.FACEMESH_TESSELATION,
        landmark_drawing_spec=None,
        connection_drawing_spec=mp.solutions.drawing_styles
        .get_default_face_mesh_tesselation_style())
    solutions.drawing_utils.draw_landmarks(
        image=annotated_image,
        landmark_list=face_landmarks_proto,
        connections=mp.solutions.face_mesh.FACEMESH_CONTOURS,
        landmark_drawing_spec=None,
        connection_drawing_spec=mp.solutions.drawing_styles
        .get_default_face_mesh_contours_style())
    solutions.drawing_utils.draw_landmarks(
        image=annotated_image,
        landmark_list=face_landmarks_proto,
        connections=mp.solutions.face_mesh.FACEMESH_IRISES,
          landmark_drawing_spec=None,
          connection_drawing_spec=mp.solutions.drawing_styles
          .get_default_face_mesh_iris_connections_style())

  return annotated_image

def plot_face_blendshapes_bar_graph(face_blendshapes):
  # Extract the face blendshapes category names and scores.
  face_blendshapes_names = [face_blendshapes_category.category_name for face_blendshapes_category in face_blendshapes]
  face_blendshapes_scores = [face_blendshapes_category.score for face_blendshapes_category in face_blendshapes]
  # The blendshapes are ordered in decreasing score value.
  face_blendshapes_ranks = range(len(face_blendshapes_names))

  fig, ax = plt.subplots(figsize=(12, 12))
  bar = ax.barh(face_blendshapes_ranks, face_blendshapes_scores, label=[str(x) for x in face_blendshapes_ranks])
  ax.set_yticks(face_blendshapes_ranks, face_blendshapes_names)
  ax.invert_yaxis()

  # Label each bar with values
  for score, patch in zip(face_blendshapes_scores, bar.patches):
    plt.text(patch.get_x() + patch.get_width(), patch.get_y(), f"{score:.4f}", va="top")

  ax.set_xlabel('Score')
  ax.set_title("Face Blendshapes")
  plt.tight_layout()
  plt.show()

# Create a face landmarker instance with the live stream mode:
def print_result(result: FaceLandmarkerResult, output_image: mp.Image, timestamp_ms: int):
    face_blendshapes = result.face_blendshapes[0]
    face_blendshapes_scores = []
    for score in face_blendshapes:
      face_blendshapes_scores.append(score.score)
    data = struct.pack("@" + "f"*52, *face_blendshapes_scores)
    sock.sendto(data, addr)
    # plot_face_blendshapes_bar_graph(face_blendshapes)
    
def line_to_triangles(lines, num_point):
  triangles = []
  list_line = list(lines)
  for i in range(num_point):
     for j in range(num_point-i-1):
        for k in range(num_point-i-j-2):
           p1 = i
           p2 = j+i+1
           p3 = j+i+k+2
           conn1 = False
           conn2 = False
           conn3 = False
           for line in list_line:
            if (line[0] == p1 and line[1] == p2) or (line[0] == p2 and line[1] == p1):
               conn1 = True
            if (line[0] == p2 and line[1] == p3) or (line[0] == p3 and line[1] == p2):
               conn2 = True
            if (line[0] == p1 and line[1] == p3) or (line[0] == p3 and line[1] == p1):
               conn3 = True
            if conn1 and conn2 and conn3:
               triangles.append([p1, p2, p3])
  return triangles

def line_to_triangles2(lines):
   triangles = []
   list_line = list(lines)
   for i in range(len(list_line)):
      line1 = list_line[i]
      for j in range(len(list_line)-i-1):
         line2 = list_line[j+i+1]
         for k in range(len(list_line)-i-j-2):
            line3 = list_line[k+j+i+2]
            allp = []
            same1 = 0
            same2 = 0
            same3 = 0
            for point1 in line1:
               for point2 in line2:
                  if point1 == point2:
                     same1 += 1
                     if not (point1 in allp):
                        allp.append(point1)
            for point1 in line2:
               for point2 in line3:
                  if (point1 == point2):
                     same2 += 1
                     if not (point1 in allp):
                        allp.append(point1)
            for point1 in line3:
               for point2 in line1:
                  if (point1 == point2):
                     same3 += 1
                     if not (point1 in allp):
                        allp.append(point1)
            if same1 == 1 and same2 == 1 and same3 == 1 and len(allp) == 3:
               #make sure not repeated since for some reason they repeat connections
               triangles.append([allp[0], allp[1], allp[2]])
   return triangles

options = FaceLandmarkerOptions(
    base_options=BaseOptions(model_asset_path=model_path),
    output_face_blendshapes=True,
    running_mode=VisionRunningMode.IMAGE)

landmarker = FaceLandmarker.create_from_options(options)
mp_image = mp.Image.create_from_file("putin.jpg")
result = landmarker.detect(mp_image)

all_float_data = []
all_int_data = []

landmarks = result.face_landmarks

for res in landmarks:
    for landmark in res:
        print(landmark)
        all_float_data.append(landmark.x)
        all_float_data.append(landmark.y)
        all_float_data.append(landmark.z)


face_connection = frozenset().union(*[mp.solutions.face_mesh.FACEMESH_LIPS, mp.solutions.face_mesh.FACEMESH_LEFT_EYE, mp.solutions.face_mesh.FACEMESH_LEFT_EYEBROW, mp.solutions.face_mesh.FACEMESH_RIGHT_EYE, mp.solutions.face_mesh.FACEMESH_RIGHT_EYEBROW, mp.solutions.face_mesh.FACEMESH_FACE_OVAL, mp.solutions.face_mesh.FACEMESH_TESSELATION])
# triangles = line_to_triangles2(face_connection)
# f = open('triangles.json', 'w', encoding="utf-8")
# f.write(json.dumps(triangles))
# f.close()


f = open('triangles.json', 'r', encoding="utf-8")
triangles = json.loads(f.readline())
f.close()


for i in range(len(triangles)):
  for j in range(3):
    all_int_data.append(triangles[i][j])

def send_landmark468():
   data = struct.pack('@' + 'f'*(468*3), *(all_float_data[0:468*3]))
   # data = struct.pack('@' + 'i'*len(all_int_data), *all_int_data)
   sock.sendto(data, addr)
   print("send landmark")

def send_connections():
   connection_list = []
   for connection in list(face_connection):
      connection_list.append(connection[0])
      connection_list.append(connection[1])
   print(len(connection_list)*4)
   data = struct.pack('@' + 'i'*(len(connection_list)), *connection_list)
   sock.sendto(data, addr)
send_landmark468()
# send_connections()