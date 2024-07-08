import { Injectable } from '@angular/core';
import notify from 'devextreme/ui/notify';

@Injectable()
export class NotificationService {

    print(message: string, type: NotificationType){
        notify({
            message: message,
            position: {
                my: "center top",
                at: "center top",
                offset: "0 20"
            },
            width: 300,
            type: type
        }, type, 5000);
    }
}

export enum NotificationType {
    Info = "info",
    Error = "error",
    Success = "success",
    Warning = "warning"
}
