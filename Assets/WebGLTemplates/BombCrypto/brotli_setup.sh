# Chạy file này để setup các config cho build webgl nén theo kiểu brotli
# Sau khi up thư mục game lên Gcloud => Chạy file này (bash brotli_setup.sh)

# Nếu build project bằng window ra thì phải tạo lại file này với nội dung tương tự và xoá file này có sẵn trong từ window đi
# và để file mới này vào và chạy, sử dụng file brotli_setup.sh có sẵn khi build từ window ra chạy sẽ bị lỗi

# GET DIRECTORY URL
echo "Nhập vào đường dẫn thư mục trên gcloud:"
read INPUT


#CHANGE CONTENT ENCODING TO BR
urls=()

# Recursively find .js, .br, and .warn files in all subfolders
while IFS= read -r -d '' file; do
    # Add file URL to the array
    file="${file#./}"
    urls+=("$file")
done < <(find . -type f \( -name '*.br' \) -print0)

# Print all URLs
for url in "${urls[@]}"; do
    echo "CHANGE CONTENT ENCODING TO br FOR $url"
    gcloud storage objects update "gs://$INPUT/$url" --content-encoding=br
done

#CHANGE CONTENT TYPE TO application/wasm
urlWasm=()

# Recursively find .js, .br, and .warn files in all subfolders
while IFS= read -r -d '' file; do
    # Add file URL to the array
    file="${file#./}"
    urlWasm+=("$file")
done < <(find . -type f \( -name '*.wasm' -o -name '*.wasm.gz' -o -name '*.wasm.br' -o -name '*.data.br' \) -print0)

# Print all URLs
for url in "${urlWasm[@]}"; do
    echo "Update content type for $url"
    gcloud storage objects update "gs://$INPUT/$url" --content-type=application/wasm
done


# CHANGE CONTENT TYPE TO application/javascript
urlJs=()

#Recursively find .js, .br, and .warn files in all subfolders
while IFS= read -r -d '' file; do
    file="${file#./}"
    # Add file URL to the array
    urlJs+=("$file")
done < <(find . -type f \( -name '*.js' -o -name '*.js.gz' -o -name '*.js.br' \) -print0)

# Print all URLs
for url in "${urlJs[@]}"; do
    echo "Update content type for JS $url"
    gcloud storage objects update "gs://$INPUT/$url" --content-type=application/javascript
done



    echo "*************_______________CONFIG COMPLETE_____________************"
