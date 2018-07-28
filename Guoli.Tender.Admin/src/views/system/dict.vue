<template>
    <section class="content-container">

        <div class="mb10">
            <el-button-group class="fl">
                <el-button v-for="t in types" v-bind:key="t.Id" @click="onTypeSelected(t.Id)">t.Name</el-button>  
            </el-button-group>
            <el-button-group class="fr">
                <el-button type="primary" icon="el-icon-plus" @click="showTypeEdit">添加字典类型</el-button>
            </el-button-group>
        </div>

        <el-dialog :title="typeTitle" :visible.sync="typeDialogVisible">
            <el-form :model="typeModel" ref="typeForm" :rules="typeRules" label-width="120px">
                <el-form-item label="类型名称" prop="name">
                    <el-input v-model="typeModel.name" @keyup.enter.native="onTypeConfirm"></el-input>
                </el-form-item>
            </el-form>
            <span slot="footer" class="dialog-footer">
                <el-button @click="dialogVisible = false">取 消</el-button>
                <el-button type="primary" @click="onTypeConfirm">确 定</el-button>
            </span>
        </el-dialog>
        
        <el-table :data="dicts" style="width: 100%;" :loading="dictLoading" border>
            <el-table-column label="序号" type="index" :width="80"></el-table-column>
            <el-table-column prop="Name" label="名称"></el-table-column>
            <el-table-column prop="Remark" label="备注"></el-table-column>
            <el-table-column prop="AddTime" label="添加时间"></el-table-column>
            <el-table-column label="操作">
                <template slot-scope="scope">
                    <el-button type="primary" size="small" icon="el-icon-edit" @click="showEdit(scope.row)">修改</el-button>
                </template>
            </el-table-column>
        </el-table>

    </section>
</template>

<script>
import NProgress from 'nprogress'
import dictApi from '@/api/dict'

export default {
    data() {
        return {
            types: [],
            dicts: [],
            dictLoading: false,
            selectedType: 0,
            // 类型编辑相关
            typeTitle: '添加类型',
            typeDialogVisible: false,
            typeModel: {
                name: ''
            },
            typeRules: {
                name: [{ required: true, message: '类型名称不能为空' },
                       { max: 20, message: '类型名称不能超过20个字' }]
            }
        }
    },

    methods: {
        loadTypes() {
            NProgress.start()
            dictApi.fetchTypes().then(res => {
                NProgress.done()
                this.types = res.data
            })
        },

        loadDicts() {
            this.dictLoading = true
            NProgress.start()
            dictApi.fetchByType(this.selectedType).then(res => {
                this.dictLoading = false
                NProgress.done()
                this.dicts = res.data
            })
        },

        showTypeEdit() {
            
        },

        showEdit(model) {

        },

        onTypeSelected(type) {
            if (this.selectedType != type) {
                this.selectedType = type
                this.loadDicts()
            }
        },

        onTypeConfirm() {
            this.$refs.typeForm.validate(valid => {
                if (valid) {

                }
            })
        }
    },

    mounted() {

    }
}
</script>

<style>

</style>
