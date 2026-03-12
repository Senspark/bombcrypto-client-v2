# Run this file to build the WebGL project

# Name of the WebGL build folder
if [ -z "$1" ]; then
  read -p "Enter the folder name of the Unity build: " FOLDER_NAME
else
  FOLDER_NAME="$1"
fi

# Path to the build output directory
ROOT_PATH="../Build/$FOLDER_NAME"

# Destination directory for copied files
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
DEST_DIR="$SCRIPT_DIR/public/webgl"

# Remove old directories
# shellcheck disable=SC2115
rm -rf "$DEST_DIR"/*

# Create a new uniquely-named folder
TIMESTAMP=$(date +%s)
NEW_FOLDER="$DEST_DIR/$TIMESTAMP"
mkdir -p "$NEW_FOLDER"

# Initialize file extension variables (varies by brotli compression)
loaderUrlExtension=""
dataUrlExtension=""
dataMobileUrlExtension=""
frameworkUrlExtension=""
codeUrlExtension=""

# Copy the StreamingAssets folder
cp -r "$ROOT_PATH/StreamingAssets" "$NEW_FOLDER/StreamingAssets"

# Navigate to the build output directory
cd "$ROOT_PATH/Build" || { echo "Build folder not found"; exit 1; }

# Copy files and assign extensions based on brotli compression
for file in *; do
  if [ -f "$file" ]; then
    EXTENSION="${file#*.}"
    #echo "File: $file, Extension: $EXTENSION"
    case "$EXTENSION" in
      loader* )
        loaderUrlExtension="/webgl.$EXTENSION"
        destFile="$loaderUrlExtension"
        ;;
      data* )
        dataUrlExtension="/webgl.$EXTENSION"
        destFile="$dataUrlExtension"
        ;;
      *js* )
        frameworkUrlExtension="/webgl.$EXTENSION"
        # Check if frameworkUrlExtension contains .framework and remove it
        if [[ "$frameworkUrlExtension" == *".framework"* ]]; then
          frameworkUrlExtension="${frameworkUrlExtension/.framework/}"
        fi
        destFile="$frameworkUrlExtension"
        ;;
      wasm* )
        codeUrlExtension="/webgl.$EXTENSION"
        destFile="$codeUrlExtension"
        ;;
    esac
    cp "$file" "$NEW_FOLDER$destFile"
    echo "Copied $file to $NEW_FOLDER$destFile"
  fi
done



cd "../" || { echo "DTX Build folder not found"; }
data_file_found=false

# Find and copy .data or .data.br files, then rename
for file in *.data *.data.br; do
  if [ -f "$file" ]; then
    data_file_found=true
    EXTENSION="${file##*.}"
    case "$EXTENSION" in
      data )
        cp "$file" "$NEW_FOLDER/mobile.data"
        dataMobileUrlExtension="/mobile.data"
        ;;
      br )
        cp "$file" "$NEW_FOLDER/mobile.data.br"
        dataMobileUrlExtension="/mobile.data.br"
        ;;
    esac
  fi
done

echo "Files copied successfully: $NEW_FOLDER"

echo "Update environment file"
# Update VITE_UNITY_FOLDER and URL_EXTENSION variables in .env files
update_env_file() {
  local env_file="$1"
  if [ -f "$env_file" ]; then
    sed -i '' "s|^VITE_UNITY_FOLDER=.*|VITE_UNITY_FOLDER=./webgl/$TIMESTAMP|g" "$env_file"
    sed -i '' "s|^VITE_LOADER_URL_EXTENSION=.*|VITE_LOADER_URL_EXTENSION=$loaderUrlExtension|g" "$env_file"
    sed -i '' "s|^VITE_DATA_URL_EXTENSION=.*|VITE_DATA_URL_EXTENSION=$dataUrlExtension|g" "$env_file"
    if [ "$data_file_found" = true ]; then
      sed -i '' "s|^VITE_DATA_URL_MOBILE_EXTENSION=.*|VITE_DATA_URL_MOBILE_EXTENSION=$dataMobileUrlExtension|g" "$env_file"
    fi
    sed -i '' "s|^VITE_FRAMEWORK_URL_EXTENSION=.*|VITE_FRAMEWORK_URL_EXTENSION=$frameworkUrlExtension|g" "$env_file"
    sed -i '' "s|^VITE_CODE_URL_EXTENSION=.*|VITE_CODE_URL_EXTENSION=$codeUrlExtension|g" "$env_file"
    echo "Updated $env_file"
  else
    echo "$env_file not found"
  fi
}

update_env_file "$SCRIPT_DIR/.env"
update_env_file "$SCRIPT_DIR/.env.production"
update_env_file "$SCRIPT_DIR/.env.test"

echo "*************_______________COMPLETE_____________**************"
