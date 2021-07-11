import React, {useEffect, useState } from 'react';
import {Button, Grid, Header} from 'semantic-ui-react';
import PhotoWidgetDropzone from "./PhotoWidgetDropzone";
import { Cropper } from 'react-cropper';
import PhotoWidgetCropper from "./PhotoWidgetCropper";

interface Props {
    loading: boolean;
    uploadPhoto: (file: Blob) => void;
}


export default function PhotoUploadWidget({loading, uploadPhoto}: Props) {
    //empty array ([]) in useState to avoid null error for Step 2
    //gave useState <any> type to avoid "preview" error of not applicable on "never" type
    const [files, setFiles] = useState<any>([]);
    const [cropper, setCropper] = useState<Cropper>();
    
    function onCrop() {
        if (cropper) {
            cropper.getCroppedCanvas().toBlob(blob => uploadPhoto(blob!));
        }
    }
    
    //useEffect() cleans up cropper component after its been disposed of
    useEffect(() => {
        return () => {
            files.forEach((file: any) => URL.revokeObjectURL(file.preview))
        }
    }, [files])
    
    //12 columns in 16-column grid w/ 1-column separators (15/16 in total)
    return (
        <Grid>
            <Grid.Column width={4}>
                <Header sub color='teal' content='Step 1 - Add Photo' />
                <PhotoWidgetDropzone setFiles={setFiles}/>
            </Grid.Column>
            <Grid.Column width={1} />
            <Grid.Column width={4}>
                <Header sub color='teal' content='Step 2 - Resize Image' />
                {files && files.length > 0 && (
                    <PhotoWidgetCropper setCropper={setCropper} imagePreview={files[0].preview}/>
                )}
            </Grid.Column>
            <Grid.Column width={1} />
            <Grid.Column width={4}>
                <Header sub color='teal' content='Step 3 - Preview and Upload' />
                {files && files.length > 0 && 
                <>
                    <div className='img-preview' style={{minHeight: 200, overflow: 'hidden' }}/>
                    <Button.Group widths={2}>
                        <Button loading={loading} onClick={onCrop} positive icon='check'/>
                        <Button diabled={loading} onClick={() => setFiles([])} icon='close'/>
                    </Button.Group>
                </>}
                
            </Grid.Column>
        </Grid>
    )
}