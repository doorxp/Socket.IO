
UNITY_NAME = Unity
UNITY_DIR  = /Volumes/doorxp/Applications/$(UNITY_NAME)/$(UNITY_NAME).app


API = $(UNITY_DIR)/Contents/MonoBleedingEdge/lib/mono/4.7.1-api

CSC             = $(UNITY_DIR)/Contents/Tools/Roslyn/csc

MANAGED_PATH = $(UNITY_DIR)/Contents/Managed
UNITYENGINE = $(MANAGED_PATH)
FACADES = $(API)/Facades

SRC_DIR     = Scripts
BUILD_DIR   = bin
TARGET_NAME = UniRx.dll

BUILD_TARGET = $(BUILD_DIR)/$(TARGET_NAME)

SRC = $(shell find $(SRC_DIR) -name '*.cs')

ifeq ($(CSC),)
$(error Mono compiler 'csc' is not found in $(UNITY_DIR))
endif

.PHONY: all build

all: build

build: $(BUILD_TARGET)

$(BUILD_TARGET): $(SRC)

	$(shell install -d $(BUILD_DIR))

	$(CSC) -out:$(BUILD_TARGET) \
  -target:library \
  -lib:$(UNITYENGINE) \
  -lib:$(API) \
  -lib:$(FACADES) \
  -r:UnityEngine.dll \
  -r:UnityEditor.dll \
  -r:System.Linq.dll \
  -r:System.Collections.dll \
  -r:System.dll \
  -r:System.Core.dll \
  -r:mscorlib.dll \
  -d:UNITY_4_5 -d:NETFX_CORE -d:Release -d:UNITY_METRO \
  $(SRC)
	@echo built: $@