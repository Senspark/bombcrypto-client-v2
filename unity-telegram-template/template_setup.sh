# Chạy file này để build ra project webgl dùng cho telegram

# Tên thư mục của project webgl vừa build
if [ -z "$1" ]; then
  read -p "Nhập tên thư mục mới build từ unity: " FOLDER_NAME
else
  FOLDER_NAME="$1"
fi

# Kiểm tra loại build (test hoặc prod)
echo "Chọn loại build:"
options=("test" "prod" "Quit")
select opt in "${options[@]}"
do
    case $opt in
        "Quit")
            exit 0
            ;;
        "test"|"prod")
            BUILD_TYPE=$opt
            break
            ;;
        *) echo "Lựa chọn không hợp lệ: $REPLY";;
    esac
done

# Đường dẫn đến thư mục chứa các file build
ROOT_PATH="../Build/$FOLDER_NAME"

# Đường dẫn đến thư mục cần copy
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
DEST_DIR="$SCRIPT_DIR/public/webgl"

# Xoá các thư mục cũ
# shellcheck disable=SC2115
rm -rf "$DEST_DIR"/*

# Tạo 1 folder mới không trùng tên
TIMESTAMP=$(date +%s)
NEW_FOLDER="$DEST_DIR/$TIMESTAMP"
mkdir -p "$NEW_FOLDER"

# Tạo các extensions tuỳ theo có nén brotli hay ko
loaderUrlExtension=""
dataUrlExtension=""
dataMobileUrlExtension=""
frameworkUrlExtension=""
codeUrlExtension=""

# Copy the StreamingAssets folder
cp -r "$ROOT_PATH/StreamingAssets" "$NEW_FOLDER/StreamingAssets"

# Di chuyển đến thư mục cần copy
cd "$ROOT_PATH/Build" || { echo "Build folder not found"; exit 1; }

# Copy và khai báo cho các extension phù hợp tuỳ theo build nén brotli hay ko
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

# Tìm và sao chép file .data hoặc .data.br, sau đó đổi tên
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

echo "Các file đã được copy thành công: $NEW_FOLDER"

echo "Update environment file"
# Cập nhật biến VITE_UNITY_FOLDER và các biến URL_EXTENSION trong các file .env
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

# Di chuyển về lại thư mục script để build
cd "$SCRIPT_DIR" || { echo "Script directory not found"; exit 1; }

echo "Bắt đầu build project..."

# Build unity project
if [ "$BUILD_TYPE" == "prod" ]; then
  npm run build-prod
else
  npm run build-test
fi

echo "Build project thành công"

# Sao chép file brotli_setup.sh vào thư mục dist nếu tồn tại (để setup cho các config nén)
if [ -f "$ROOT_PATH/brotli_setup.sh" ]; then
  cp "$ROOT_PATH/brotli_setup.sh" "$SCRIPT_DIR/dist/brotli_setup.sh"
  echo "brotli_setup.sh đã được sao chép vào thư mục dist"
else
  echo "ko tìm thấy file brotli_setup.sh trong thư mục $ROOT_PATH"
fi

echo "*************_______________COMPLETE_____________**************"