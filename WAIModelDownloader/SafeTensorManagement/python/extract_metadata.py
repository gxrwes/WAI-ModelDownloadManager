import sys
import safetensors

def extract_metadata(file_path):
    metadata = safetensors.safe_open(file_path)
    
    # Extract tensor names and shapes
    tensors_info = {key: value.shape for key, value in metadata.items()}
    
    # Extract data types
    dtypes = {key: value.dtype for key, value in metadata.items()}
    
    # Construct the result as a dictionary
    result = {
        "tensors_info": tensors_info,
        "dtypes": dtypes,
    }
    
    return result

if __name__ == "__main__":
    file_path = sys.argv[1]
    metadata = extract_metadata(file_path)
    print(metadata)
