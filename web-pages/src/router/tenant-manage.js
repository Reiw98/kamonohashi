import ManageTenant from '@/views/tenant-manage/tenant/Setting'
import ManageUserIndex from '@/views/tenant-manage/user/Index'
import ManageUserEdit from '@/views/tenant-manage/user/Edit'
import ManageResourceIndex from '@/views/tenant-manage/resource/Index'

export default [
  {
    path: '/manage/tenant',
    component: ManageTenant,
  },
  {
    path: '/manage/user',
    component: ManageUserIndex,
    children: [
      {
        path: ':id',
        component: ManageUserEdit,
        props: true,
      },
    ],
  },
  {
    path: '/manage/resource',
    component: ManageResourceIndex,
  },
]
